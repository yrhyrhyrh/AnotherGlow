# Stage 1: Build Angular App
FROM node:18 AS build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod

# Stage 2: Serve app with Nginx
FROM nginx:alpine
COPY --from=build /app/dist/app-frontend /usr/share/nginx/html
