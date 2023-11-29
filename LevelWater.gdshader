shader_type canvas_item;

uniform float wave_speed = 3.0; //wave loop speed
uniform float wave_freq = 10.0; //wave vertical freq
uniform float wave_width = 1; //wave width 



uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

varying vec2 globalposition;
uniform sampler2D refraction_map;
uniform vec2 refraction_stretch = vec2(1.0, 1.0);
uniform float refraction_strength : hint_range(0.0, 0.1) = 0.02;
uniform vec4 water_tint : source_color = vec4(0.2, 0.6, 1.0, 0.1);
uniform float speed : hint_range(0.0, 1.0) = 0.1;


void fragment(){
	vec2 scale_UV = SCREEN_UV;
	vec2 wave_uv_offset;
	wave_uv_offset.x = cos((TIME * wave_speed) + UV.x + UV.y * wave_freq * 2.0) * wave_width * 0.01;
	//COLOR = vec4(wave_uv_offset,0.0,1.0);
	
	vec2 uv = globalposition / 512.0;
	
	vec2 refraction_offset = texture(
		refraction_map, 
		vec2(
			mod(uv.x * refraction_stretch.x + TIME * speed, 1.0), 
			mod(uv.y * refraction_stretch.y + TIME * speed, 1.0))).xy;
	// Set values between -0.5 and 0.5 (instead of 0 and 1). Otherwise the reflection will move whith increased refraction_strength
	refraction_offset -= 0.5; 
	
	
	// Get the screen texture and distort it
	vec4 refraction = texture(SCREEN_TEXTURE, SCREEN_UV - refraction_offset * refraction_strength);
	
	vec4 color = vec4(1.0);
	color.rgb = mix(refraction.rgb, water_tint.rgb, water_tint.a);
	
	vec4 wave_color = texture(SCREEN_TEXTURE, scale_UV + wave_uv_offset);
	vec4 final_color;
	final_color = mix(color, wave_color, .55);
	final_color.a = mix(texture(TEXTURE, UV).a, final_color.a, .7);
	
	final_color.rgb = mix(final_color.rgb, texture(TEXTURE, UV).rgb, .6);
	COLOR = final_color;
	
	
}


void vertex() {
	globalposition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}