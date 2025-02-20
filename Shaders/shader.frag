#version 330 core

in vec2 TexCoord;
in vec3 Color;
out vec4 FragColor;
uniform sampler2D uTexture;

void main()
{
    FragColor = vec4(Color, 1);
}
