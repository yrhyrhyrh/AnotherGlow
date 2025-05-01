export interface GroupData {
  GroupId: string;
  Name: string;
  Description: string;
  GroupPictureUrl?: string;
  IsAdmin: boolean;
  Members: Array<{
    GroupMemberId: string;
    IsAdmin: boolean;
    User: {
      UserId: string;
      Username: string;
      ProfilePictureUrl: string;
    };
  }>;
} 