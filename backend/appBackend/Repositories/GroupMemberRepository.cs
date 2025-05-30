using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{

    public interface IGroupMemberRepository
    {
        Task<bool> AddGroupMembersAsync(List<GroupMember> newGroupMembers); // Get group by creds
        Task<bool> RemoveMemberAsync(Guid groupMemberId);
        Task<bool> MakeAdminAsync(Guid groupMemberId);
    }

    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly SocialMediaDbContext _context;

        public GroupMemberRepository(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddGroupMembersAsync(List<GroupMember> newGroupMembers)
        {
            if (!newGroupMembers.Any()) 
                return false; // No members to add, return false
            
            try
            {
                _context.GroupMembers.AddRange(newGroupMembers);
                await _context.SaveChangesAsync();
                return true; // Successfully added members
            }
            catch (DbUpdateException ex) 
            {
                // Log the database error (e.g., foreign key constraint)
                Console.WriteLine($"Database update failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            
            return false; // Return false if an error occurs
        }

        public async Task<bool> RemoveMemberAsync(Guid groupMemberId)
        {
            var member = await _context.GroupMembers.FindAsync(groupMemberId);
            if (member == null) return false;
            
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MakeAdminAsync(Guid groupMemberId)
        {
            var member = await _context.GroupMembers.FindAsync(groupMemberId);
            if (member == null) return false;

            member.IsAdmin = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
