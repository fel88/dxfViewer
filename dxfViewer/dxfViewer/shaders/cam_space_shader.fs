#version 330 core
out vec4 FragColor;



in vec3 FragPos;  

uniform vec3 color;
  
uniform vec3 viewPos;


void main()
{
        
        
    vec3 result = color;
    FragColor = vec4(result, 1.0);
} 

