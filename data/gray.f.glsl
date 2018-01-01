#version 330 core
in  vec2 texcoord;
out vec4 fs_FragColor;
uniform sampler2D source;

void main()
{
	vec4   color = texture(source, texcoord);
	float    avg = 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
	fs_FragColor = vec4(avg, avg, avg, 1);
}
