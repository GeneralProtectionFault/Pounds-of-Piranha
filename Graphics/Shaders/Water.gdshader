shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform vec4 water_color : source_color;
uniform sampler2D noise;

uniform vec2 scale;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	float uv_height = SCREEN_PIXEL_SIZE.y / TEXTURE_PIXEL_SIZE.y;
	
	float distortion = texture(noise, UV * scale).x;
	vec2 new_uv = vec2(SCREEN_UV.x - distortion, SCREEN_UV.y);
	
	// COLOR = texture(SCREEN_TEXTURE, new_uv);
	COLOR.rgb = mix(COLOR.rgb, water_color.rgb, water_color.a);;
	//UV.x -= distortion;
}

void light() {
	// Called for every pixel for every light affecting the CanvasItem.
}
