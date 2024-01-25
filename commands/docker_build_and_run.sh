#!/usr/bin/env zsh
cd ../backend/
docker build -t uldahlalex/ws:latest .  
docker run --env-file .env -p 8181:8181 uldahlalex/ws
