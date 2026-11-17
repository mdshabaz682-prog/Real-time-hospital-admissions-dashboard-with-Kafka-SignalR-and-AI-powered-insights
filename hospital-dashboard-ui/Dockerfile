FROM node:22 AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npx ng build --configuration production

FROM nginx:alpine
COPY default.conf.template /etc/nginx/templates/default.conf.template
ENV API_UPSTREAM=http://api:8080
COPY --from=build /app/dist/hospital-dashboard-ui/browser /usr/share/nginx/html
EXPOSE 80