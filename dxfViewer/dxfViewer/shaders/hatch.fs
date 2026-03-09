#version 330 core
out vec4 FragColor;

uniform vec3 color;

 uniform vec3 bgColor;      // Background color

void main()
{	
	

 vec3 hatchColor=color;   // The single color for lines

 float density=10;     // Line density
 float lineWidth=2;   // Line width
	// Calculate hatching based on screen coordinates (x+y for diagonal)
    float hatchPattern = mod(gl_FragCoord.x + gl_FragCoord.y, density);
    
    // If within line width, apply color; otherwise, bg color
    if (hatchPattern < lineWidth) {
        FragColor = vec4(hatchColor, 1.0);
    } else {
        FragColor = vec4(bgColor, 1.0);
    }
}