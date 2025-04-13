// src/app/models/dto.ts
export interface PostDTO {
    PostId: string;
    UserId: string;
    AuthorUsername: string;
    AuthorFullName: string;
    Content: string;
    CreatedAt: Date;
    LikeCount: number;
    CommentCount: number;
    Attachments: AttachmentDTO[];
  }
  
  export interface CreatePostRequestDTO {
    Content: string;
    Attachments?: File[]; // Assuming File type for Attachments
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