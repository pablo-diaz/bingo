server {
  listen 80 default_server;
  listen [::]:80 default_server;

  location / {
    proxy_pass http://bingoserver:80;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "Upgrade";
    proxy_set_header Host $host;
  }

  location = /404.html {
    internal;
  }
}