# Stage 1: Build Angular App
FROM node:lts-alpine as builder
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build --configuration=production

# Stage 2: Serve app with Nginx
FROM nginx:alpine
COPY --from=builder /app/dist/app-frontend/browser /usr/share/nginx/html

# Use custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
