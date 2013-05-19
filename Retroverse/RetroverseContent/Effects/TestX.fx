//--------------------------------------------------------------------------------------
// File: virtual_vcr_shaderx4.fx
//
// The effect file for the virtual_vcr_shaderx4 sample.  
// 
// Copyright (c) Joachim Diepstraten 2005. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float	 g_fTime;					// App's time in seconds
float4x4 g_mWorld;					// World matrix for object
float4x4 g_mWorldViewProjection;	// World * View * Projection matrix
float3x3 g_mInvTMV;
float3   g_lightPos;
float3   g_eyePos;
texture  g_sourceFrame;
texture  g_sourceFrameNext;
texture  g_imageNoise;
texture  g_jitter;
texture  g_luminanceArea;
texture  g_barrier;

struct VS_OUTPUT {
  float4 Diffuse : COLOR;
  float4 Position : POSITION;
  float3 Normal   : TEXCOORD0;
  float3 lightDir : TEXCOORD1;
  float3 viewDir  : TEXCOORD2;
  float2 TexCoord : TEXCOORD3;
};


VS_OUTPUT PhongShade(float4 Position : POSITION,
                     float3 Normal : NORMAL) {

  VS_OUTPUT Out = (VS_OUTPUT)0;
  float4 WorldPos = mul(Position,g_mWorld);
  float3 lightDir = WorldPos.xyz - g_lightPos.xyz;
  float3 viewDir = WorldPos.xyz - g_eyePos.xyz;
  lightDir = normalize(lightDir);
  float3 newNormal = mul(Normal,g_mInvTMV);
  newNormal = normalize(newNormal);
  Out.Position = mul(Position,g_mWorldViewProjection);
  Out.Diffuse = dot(lightDir,newNormal);                     
  Out.Normal = newNormal;
  Out.lightDir = lightDir;
  Out.viewDir = -viewDir;
  return Out;
}


PixelShader BlinnPS = asm {
  ps_2_0
  dcl v0
  dcl t0
  dcl t1
  dcl t2
  def c1, 0.5, 16.0, 0.0, 0.0
  mul r0, v0, c0  
  nrm r1, t0
  nrm r2, t1
  nrm r3, t2
  add r2, r2, r3
  mul r2, r2, c1.x
  dp3_sat r3, r1, r2
  pow r2, r3.x, c1.y
  mul r2, r2, c1.y
  cmp r2, v0, r2, c2.w
  add r0, r0, r2
  mov oC0, r0
};

PixelShader VCR_NormalPlaybackPS = asm {
  ps_2_0
  dcl t0
  dcl_2d s0
  dcl_2d s3
  def c0, 0.5, 0.5, 0.5, 0.5
  def c1, 0.59, 0.3, 0.2, 0.0
  def c2, 0.39, 0.4, 0.3, 0.0
  def c3, 0.3, 0.6, 0.3, 0.0
  def c4, 0.6, 1.2, 0.0, 0.0
  def c5, 0.299, 0.587, 0.114, 0.0
  def c6, 0.0, 0.565, 0.713, 0.0
  def c7, 1.403, 0.344, 0.714, 1.770
  def c8, 1.0, 0.25, 0.5, 0.0
  def c9, 1.0, 0.0, 0.0, 0.0
  texld r1, t0,s0
  // convert to YUV
  dp3 r2, r1, c5 
  sub r2.y, r1.z, r2.x 
  sub r2.z, r1.x, r2.x
  mul r2.yz, r2, c6
  // add well-known yittering to YUV
  texld r4, t0, s3 // load white noise
  mul r4, r4, c29.x // multiply by noise scale factor
  add r2, r2, r4 // add white noise to YUV signal

// convert back to RGB
  mad r1.x, r2.z, c7.x, r2.x
  mad r1.y, r2.y, -c7.y, r2.x
  mad r1.y, r2.z, -c7.z, r1.y
  mad r1.z, r2.y, c7.w, r2.x
  mov oC0, r1
};

PixelShader VCR_fastforwardPS = asm {
  ps_2_0
  dcl t0
  dcl_2d s0
  dcl_2d s1
  dcl_2d s2
  dcl_2d s3
  dcl_2d s4
  dcl_2d s5
  def c0, 0.5, 0.5, 0.5, 0.5
  def c1, 0.59, 0.3, 0.2, 0.0
  def c2, 0.39, 0.4, 0.3, 0.0
  def c3, 0.3, 0.6, 0.3, 0.0
  def c4, 0.6, 1.2, 0.0, 0.0
  def c5, 0.299, 0.587, 0.114, 0.0
  def c6, 0.0, 0.565, 0.713, 0.0
  def c7, 1.403, 0.344, 0.714, 1.770
  def c8, 1.0, 0.25, 0.5, 0.0
  def c9, 1.0, 0.0, 0.0, 0.0
  add r11, t0, c30
  add r10, t0, c31
  // Add overflow non pow2 fix here
  sub r8, c8.x, r10.x
  abs r9, r8
  cmp r10.x, r8.x, r10, r9
  //
  add r1, t0, c10
  add r2, t0, c11
  texld r9, r1, s4 // load barrier texture
  texld r8, r2, s4 // load barrier texture with different texture coords
  min r9, r8, r9 // mask area
  texld r0, r11, s1 // load shifter
  texld r10, r10, s2 // luminance area
  add r0, r0, -c0 // compute sign for direction 
  mul r0, r0, r9.x // scale factor
  add r1, r0, t0 // add shift to texture coordinate
  mul r10, r10, r9.x // scale factor?
  sub r2, r1, c9  // overflow 1-t(x)
  texld r1, r1, s0 // load addon texture
  texld r3, r2, s5 // load from next frame for overflow 
  cmp r1, r2.x, r3, r1 // check if overflow
  // convert to YUV
  dp3 r2, r1, c5
  sub r2.y, r1.z, r2.x 
  sub r2.z, r1.x, r2.x
  mul r2.yz, r2, c6
  // add well-known yittering to YUV
  texld r4, t0, s3 // load white noise
  mul r4, r4, c29.x // multiply by noise scale factor
  add r2, r2, r4 // add white noise to YUV signal
  sub r8, c0.x, r10.x // check masking
  add r5, c8, -r8.x // is there a luminance change?
  mul r3, r2, r5 // if yes use this for scaling
  cmp r2, r8.x, r2, r3 // is there luminance change if yes use scaled luminance otherwise "normal"

  // convert back to RGB
  mad r1.x, r2.z, c7.x, r2.x
  mad r1.y, r2.y, -c7.y, r2.x
  mad r1.y, r2.z, -c7.z, r1.y
  mad r1.z, r2.y, c7.w, r2.x

  mov oC0, r1
};

PixelShader VCR_rewindPS = asm {
  ps_2_0
  dcl t0
  dcl_2d s0
  dcl_2d s1
  dcl_2d s2
  dcl_2d s3
  dcl_2d s4
  dcl_2d s5
  def c0, 0.5, 0.5, 0.5, 0.5
  def c1, 0.59, 0.3, 0.2, 0.0
  def c2, 0.39, 0.4, 0.3, 0.0
  def c3, 0.3, 0.6, 0.3, 0.0
  def c4, 0.6, 1.2, 0.0, 0.0
  def c5, 0.299, 0.587, 0.114, 0.0
  def c6, 0.0, 0.565, 0.713, 0.0
  def c7, 1.403, 0.344, 0.714, 1.770
  def c8, 1.0, 0.25, 0.5, 0.0
  def c9, 1.0, 0.0, 0.0, 0.0
  add r11, t0, c30
  add r10, t0, c31
  // Add underflow non pow2 fix here
  add r9, r10.x, c8.x
  cmp r10.x, r10.x, r10, r9
  //
  add r1, t0, c10
  add r2, t0, c11
  texld r9, r1, s4
  texld r8, r2, s4
  min r9, r8, r9
  texld r0, r11, s1
  texld r10, r10, s2
  add r0, r0, -c0
  mul r0, r0, r9.x
  add r1, -r0.xyzw, t0
  mul r10, r10, r9.x
  add r2, c9, r1
  texld r1, r1, s0
  texld r3, r2, s5
  cmp r1, r1.x, r1, r3
  // convert to YUV
  dp3 r2, r1, c5
  sub r2.y, r1.z, r2.x 
  sub r2.z, r1.x, r2.x
  mul r2.yz, r2, c6
  // add well-known yittering to YUV
  texld r4, t0, s3
  mul r4, r4, c29.x // multiply by noise scale factor
  add r2, r2, r4
  sub r8, c0.x, r10.x
  add r5, c8, -r8.x
  mul r3, r2, r5
  cmp r2, r8.x, r2, r3
  // convert back to RGB
  mad r1.x, r2.z, c7.x, r2.x
  mad r1.y, r2.y, -c7.y, r2.x
  mad r1.y, r2.z, -c7.z, r1.y
  mad r1.z, r2.y, c7.w, r2.x

  mov oC0, r1
};

PixelShader VCR_stillPS = asm {
  ps_2_0
  dcl t0
  dcl_2d s0
  dcl_2d s3
  dcl_2d s5
  def c0, 0.5, 0.5, 0.5, 0.5
  def c1, 0.59, 0.3, 0.2, 0.0
  def c2, 0.39, 0.4, 0.3, 0.0
  def c3, 0.3, 0.6, 0.3, 0.0
  def c4, 0.6, 1.2, 0.0, 0.0
  def c5, 0.299, 0.587, 0.114, 0.0
  def c6, 0.0, 0.565, 0.713, 0.0
  def c7, 1.403, 0.344, 0.714, 1.770
  def c8, 1.0, 0.25, 0.5, 0.0
  def c9, 1.0, 0.0, 0.0, 0.0
  texld r1, t0, s0
  texld r3, t0, s5
  mul r3, r3, c10.x
  mul r1, r1, c10.y
  add r1, r1, r3
// convert to YUV
  dp3 r2, r1, c5
  sub r2.y, r1.z, r2.x 
  sub r2.z, r1.x, r2.x
  mul r2.yz, r2, c6
// add well-known yittering to YUV
  texld r4, t0, s3
  mul r4, r4, c29.x // multiply by noise scale factor
  add r2, r2, r4

// convert back to RGB
  mad r1.x, r2.z, c7.x, r2.x
  mad r1.y, r2.y, -c7.y, r2.x
  mad r1.y, r2.z, -c7.z, r1.y
  mad r1.z, r2.y, c7.w, r2.x
  mov oC0, r1
};

//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {  
      VertexShader = compile vs_2_0 PhongShade();
      PixelShader = (BlinnPS);
    }
}

technique VCRPlayback
{
   pass P0
   {
     PixelShader = (VCR_NormalPlaybackPS);
     Texture[0] = (g_sourceFrame);
     Texture[3] = (g_imageNoise);    
     CULLMODE = NONE;
   }
}

technique VCRFastforward
{
   pass P0
   {
     PixelShader = (VCR_fastforwardPS);
     Texture[0] = (g_sourceFrame);
     Texture[1] = (g_jitter);
     Texture[2] = (g_luminanceArea);
     Texture[3] = (g_imageNoise);   
     Texture[4] = (g_barrier);
     Texture[5] = (g_sourceFrameNext); 
     CULLMODE = NONE;
   }
}

technique VCRRewind
{
   pass P0
   {
     PixelShader = (VCR_rewindPS);
     Texture[0] = (g_sourceFrame);
     Texture[1] = (g_jitter);
     Texture[2] = (g_luminanceArea);
     Texture[3] = (g_imageNoise);   
     Texture[4] = (g_barrier);
     Texture[5] = (g_sourceFrameNext); 
     CULLMODE = NONE;
   }
}

technique VCRStill
{
   pass P0
   {
     PixelShader = (VCR_stillPS);
     Texture[0] = (g_sourceFrame);
     Texture[3] = (g_imageNoise);   
     Texture[5] = (g_sourceFrameNext); 
     CULLMODE = NONE;
   }
}