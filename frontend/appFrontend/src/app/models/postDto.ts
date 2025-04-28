// src/app/models/dto.ts
export interface PostDTO {
    PostId: string;
    UserId: string;
    GroupId: string;
    AuthorUsername: string;
    AuthorFullName: string;
    Content: string;
    CreatedAt: Date;
    LikeCount: number;
    CommentCount: number;
    IsLikedByCurrentUser: boolean;
    Attachments: AttachmentDTO[];
    Poll: Poll;
  }
  
  export interface CreatePostRequestDTO {
    GroupId: string;
    Content: string;
    Attachments?: File[]; // Assuming File type for Attachments
    Poll?: Poll;
  }
  
  export interface UpdatePostRequestDTO {
    Content?: string;
  }
  
  export interface CommentDTO {
    CommentId: string;
    PostId: string;
    UserId: string;
    AuthorUsername: string;
    AuthorFullName: string;
    Content: string;
    CreatedAt: Date;
  }
  
  export interface CreateCommentRequestDTO {
    Content: string;
  }
  
  export interface AttachmentDTO {
    AttachmentId: string;
    FileName: string;
    FilePath: string;
    ContentType: string;
    FileSize: number;
  }
  
  export interface LikeDTO {
    LikeId: string;
    PostId: string;
    UserId: string;
    CreatedAt: Date;
  }

  export interface Poll {
    Question: string;
    Options: string[]; // Array of options
    IsGlobal: boolean;
    AllowMultipleSelections: boolean;
  }
  