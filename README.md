# AnotherGlow
create ecr:
aws ecr create-repository --repository-name myapp-backend
aws ecr create-repository --repository-name myapp-frontend


connect docker to ecr:
aws ecr get-login-password --region ap-southeast-1 | docker login --username AWS --password-stdin 650251725962.dkr.ecr.ap-southeast-1.amazonaws.com

docker build -t myapp-backend .
docker tag myapp-backend:latest 650251725962.dkr.ecr.ap-southeast-1.amazonaws.com/myapp-backend:latest
docker push 650251725962.dkr.ecr.ap-southeast-1.amazonaws.com/myapp-backend:latest

docker build -t myapp-frontend .
docker tag myapp-frontend:latest 650251725962.dkr.ecr.ap-southeast-1.amazonaws.com/myapp-frontend:latest
docker push 650251725962.dkr.ecr.ap-southeast-1.amazonaws.com/myapp-frontend:latest