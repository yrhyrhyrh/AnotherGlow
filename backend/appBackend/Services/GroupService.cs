using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using appBackend.Adapters;
using appBackend.Dtos.Group;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace appBackend.Services
{
    public class GroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupAdapter _groupAdapter;
        private readonly IAmazonS3 _s3Client;
        private readonly string _s3BucketName;
        private readonly string _cloudformS3domain;

        public GroupService(
            IGroupRepository groupRepository, 
            IGroupAdapter groupAdapter,
            IAmazonS3 s3Client,
            IConfiguration configuration
        )
        {
            _groupRepository = groupRepository;
            _groupAdapter = groupAdapter;
            _s3Client = s3Client;
            _s3BucketName = configuration["Aws:S3BucketName"] ?? throw new InvalidOperationException("S3BucketName is not configured.");
            _cloudformS3domain = configuration["Aws:CfDistributionDomainName"] ?? throw new InvalidOperationException("CfDistributionDomainName is not configured.");
        }

        public async Task<List<Group>> GetGroupsByUserIdAsync(GetGroupsByUseridRequest request)
        {
            Console.WriteLine("Getting groups where I am"+(!request.IsAdmin?" not":"")+" admin");
            Console.WriteLine(request.UserId);

            var groups = await _groupRepository.GetGroupsByUserIdAsync(request.UserId, request.IsAdmin);

            return groups;
        }

        public async Task<GroupDto?> GetGroupAsync(Guid group_id, Guid currentUserId)
        {
            Console.WriteLine("Getting group details");
            Console.WriteLine(group_id);

            var group = await _groupRepository.GetGroupAsync(group_id, currentUserId);

            return group;
        }

        public async Task<Guid> CreateGroupAsync(CreateNewGroupRequest groupRequest)
        {
            Console.WriteLine("new group name");
            Console.WriteLine(groupRequest.Name);
            
            string? groupPictureUrl = null;
            if (groupRequest.GroupPicture != null)
            {
                groupPictureUrl = await UploadGroupPictureToS3Async(groupRequest.GroupPicture);
            }

            var groupEntity = _groupAdapter.ToGroup(groupRequest);
            groupEntity.GroupPictureUrl = groupPictureUrl;

            var createdGroup_Id = await _groupRepository.CreateGroupAsync(groupEntity);

            return createdGroup_Id;
        }

        private async Task<string?> UploadGroupPictureToS3Async(IFormFile file)
        {
            if (file.Length == 0) return null;

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string s3ObjectKey = $"group-pictures/{uniqueFileName}";

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = _s3BucketName,
                    Key = s3ObjectKey,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType,
                    AutoCloseStream = true
                };

                var response = await _s3Client.PutObjectAsync(putObjectRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return $"https://{_cloudformS3domain}/{s3ObjectKey}";
                }
                else
                {
                    Console.WriteLine($"Error uploading {file.FileName} to S3. Status Code: {response.HttpStatusCode}");
                    return null;
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error uploading {file.FileName}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserDto>> SearchUsersNotInGroupAsync(Guid group_id, string keyword)
        {
            Console.WriteLine("Searching users not in group");
            var users = await _groupRepository.SearchUsersNotInGroupAsync(group_id, keyword);
            return users;
        }
    }
}
