# Build context is the repo root (needs frontend/ AND this folder's nginx.conf).
# Build with (from repo root): docker build -f nginx/Dockerfile -t taskflow-nginx .

FROM node:20 AS frontend-build

WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm install

COPY frontend/ .

# Baked in at build time (Vite inlines env vars into the JS bundle). Empty string
# makes the frontend call relative paths (e.g. /api/tasks) instead of an absolute
# host:port, so requests go through this same Nginx instead of bypassing it.
ENV VITE_API_BASE_URL=""
RUN npm run build

FROM nginx:alpine AS final

COPY nginx/nginx.conf /etc/nginx/nginx.conf
COPY --from=frontend-build /app/dist /usr/share/nginx/html

EXPOSE 80
