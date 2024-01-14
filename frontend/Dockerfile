# Stage 1: Compile and build the application
FROM node:20-alpine as build

WORKDIR /app

COPY package*.json /app/

RUN npm install

COPY . /app

RUN npm install -g @angular/cli

# Use npx to run ng serve
CMD ["npx", "ng", "serve", "--host", "0.0.0.0"]
