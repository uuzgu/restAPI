#!/bin/bash

# Update category names
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/0 -H "Content-Type: application/json" -d '{"id":0,"name":"Promotions"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/1 -H "Content-Type: application/json" -d '{"id":1,"name":"Pizza"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/2 -H "Content-Type: application/json" -d '{"id":2,"name":"Bowls"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/3 -H "Content-Type: application/json" -d '{"id":3,"name":"Hamburgers"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/4 -H "Content-Type: application/json" -d '{"id":4,"name":"Salads"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/5 -H "Content-Type: application/json" -d '{"id":5,"name":"Breakfast"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/6 -H "Content-Type: application/json" -d '{"id":6,"name":"Drinks"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/7 -H "Content-Type: application/json" -d '{"id":7,"name":"Soups"}'
curl -X PUT https://restaurant-api-923e.onrender.com/api/categories/8 -H "Content-Type: application/json" -d '{"id":8,"name":"Desserts"}' 