-- Ensure the 'public' schema exists
CREATE SCHEMA IF NOT EXISTS public;

-- Set search path to use the 'public' schema
SET search_path TO public;

-- users Table
CREATE TABLE public.users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    bio TEXT,
    profile_picture_url VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE UNIQUE INDEX users_username_unique ON public.users (username);
CREATE UNIQUE INDEX users_email_unique ON public.users (email);
CREATE INDEX users_created_at_idx ON public.users (created_at);


-- posts Table
CREATE TABLE public.posts (
    post_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
    FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE
);

CREATE INDEX posts_user_id_idx ON public.posts (user_id);
CREATE INDEX posts_created_at_idx ON public.posts (created_at);
CREATE INDEX posts_updated_at_idx ON public.posts (updated_at); -- Added for performance


-- follows Table
CREATE TABLE public.follows (
    follow_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    follower_user_id UUID NOT NULL,
    following_user_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    FOREIGN KEY (follower_user_id) REFERENCES public.users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (following_user_id) REFERENCES public.users(user_id) ON DELETE CASCADE
);

CREATE INDEX follows_follower_user_id_idx ON public.follows (follower_user_id);
CREATE INDEX follows_following_user_id_idx ON public.follows (following_user_id);


-- likes Table
CREATE TABLE public.likes (
    like_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    post_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    UNIQUE (user_id, post_id),
    FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (post_id) REFERENCES public.posts(post_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX likes_user_id_post_id_unique ON public.likes (user_id, post_id);
CREATE INDEX likes_post_id_idx ON public.likes (post_id);
CREATE INDEX likes_user_id_idx ON public.likes (user_id);


-- groups Table
CREATE TABLE public.groups (
    group_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,  -- Limited to 100 characters as per model
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);


-- group_members Table
CREATE TABLE public.group_members (
    group_member_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),  -- Matches GroupMemberId in C# model
    user_id UUID NOT NULL,
    group_id UUID NOT NULL,
    is_admin BOOLEAN NOT NULL DEFAULT FALSE,  -- Matches IsAdmin in C# model
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (group_id) REFERENCES public.groups(group_id) ON DELETE CASCADE
);

CREATE INDEX group_members_group_id_idx ON public.group_members (group_id);
CREATE INDEX group_members_user_id_idx ON public.group_members (user_id);


-- Create comments table
CREATE TABLE IF NOT EXISTS comments (
    comment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID NOT NULL,
    user_id UUID NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now()
);

-- Add indexes for comments table
CREATE INDEX IF NOT EXISTS comments_post_id_idx ON comments (post_id);
CREATE INDEX IF NOT EXISTS comments_user_id_idx ON comments (user_id);
CREATE INDEX IF NOT EXISTS comments_created_at_idx ON comments (created_at);

-- Add foreign key constraints for comments table
ALTER TABLE comments
    ADD CONSTRAINT FK_comments_posts
    FOREIGN KEY (post_id)
    REFERENCES posts(post_id)
    ON DELETE CASCADE;

ALTER TABLE comments
    ADD CONSTRAINT FK_comments_users
    FOREIGN KEY (user_id)
    REFERENCES users(user_id)
    ON DELETE CASCADE;

-- Create attachments table
CREATE TABLE IF NOT EXISTS attachments (
    attachment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(255) NOT NULL,
    content_type VARCHAR(100),
    file_size BIGINT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now()
);

-- Add indexes for attachments table
CREATE INDEX IF NOT EXISTS attachments_post_id_idx ON attachments (post_id);
CREATE INDEX IF NOT EXISTS attachments_created_at_idx ON attachments (created_at);

-- Add foreign key constraint for attachments table
ALTER TABLE attachments
    ADD CONSTRAINT FK_attachments_posts
    FOREIGN KEY (post_id)
    REFERENCES posts(post_id)
    ON DELETE CASCADE;


-- Add the group_id column to the posts table
ALTER TABLE posts
ADD COLUMN group_id UUID NOT NULL; -- make NOT NULL after truncating data

-- Add foreign key constraint
ALTER TABLE posts
ADD CONSTRAINT FK_posts_groups
FOREIGN KEY (group_id)
REFERENCES groups(group_id)
ON DELETE CASCADE; -- Choose appropriate ON DELETE behavior (CASCADE, NO ACTION, RESTRICT)

-- Add the post_id column to the posts table
ALTER TABLE polls
ADD COLUMN post_id UUID NOT NULL; -- make NOT NULL after truncating data

-- Add foreign key constraint
ALTER TABLE polls
ADD CONSTRAINT FK_polls_posts
FOREIGN KEY (post_id)
REFERENCES posts(post_id)
ON DELETE CASCADE; -- Choose appropriate ON DELETE behavior (CASCADE, NO ACTION, RESTRICT)

-- Step 1: Drop the UserId column from the "polls" table
ALTER TABLE polls
DROP COLUMN user_id;
