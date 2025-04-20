-- Add Description column to the groups table
ALTER TABLE public.groups
ADD COLUMN description TEXT;

-- Add GroupPictureUrl column to the groups table
ALTER TABLE public.groups
ADD COLUMN group_picture_url VARCHAR(255);
