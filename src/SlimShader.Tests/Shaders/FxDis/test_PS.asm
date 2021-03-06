//
// Generated by Microsoft (R) HLSL Shader Compiler 9.30.9200.20714
//
//
///
// Note: SHADER WILL ONLY WORK WITH THE DEBUG SDK LAYER ENABLED.
//
//
// Buffer Definitions: 
//
// cbuffer cbuf0
// {
//
//   float4 cool;                       // Offset:    0 Size:    16
//   int4 zeek;                         // Offset:   16 Size:    16
//   int2 arr[127];                     // Offset:   32 Size:  2024
//
// }
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim Slot Elements
// ------------------------------ ---------- ------- ----------- ---- --------
// samp0                             sampler      NA          NA    0        1
// samp1                             sampler      NA          NA    1        1
// tex0                              texture  float4          2d    0        1
// tex1                              texture  float4        cube    1        1
// tex2                              texture  float4          3d    2        1
// tex3                              texture  float4       2dMS2    3        1
// tex4[0]                           texture  float4          2d    4        1
// tex4[1]                           texture  float4          2d    5        1
// cbuf0                             cbuffer      NA          NA    0        1
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// TEXCOORD                 0   xyzw        0     NONE   float    yzw
// TEXCOORD                 1   xyzw        1     NONE   float   x   
// SV_POSITION              0   xyzw        2      POS   float   x   
// SV_RenderTargetArrayIndex     0   x           3  RTINDEX    uint   x   
// SV_ViewportArrayIndex     0    y          3  VPINDEX    uint    y  
// SV_PrimitiveID           0     z         3   PRIMID    uint     z 
// SV_CullDistance          0   x           4  CULLDST   float   x   
// SV_ClipDistance          0    y          4  CLIPDST   float    y  
// SV_SampleIndex           0   x           5   SAMPLE    uint   x   
// SV_IsFrontFace           0    y          5    FFACE    uint    y  
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_TARGET                0   xyzw        0   TARGET   float   xyzw
//
// Pixel Shader runs at sample frequency
//
ps_5_0
dcl_globalFlags refactoringAllowed
dcl_constantbuffer cb0[129], dynamicIndexed
dcl_sampler s0, mode_default
dcl_sampler s1, mode_default
dcl_resource_texture2d (float,float,float,float) t0
dcl_resource_texturecube (float,float,float,float) t1
dcl_resource_texture3d (float,float,float,float) t2
dcl_resource_texture2dms(2) (float,float,float,float) t3
dcl_resource_texture2d (float,float,float,float) t4
dcl_resource_texture2d (float,float,float,float) t5
dcl_input_ps linear v0.yzw
dcl_input_ps linear centroid v1.x
dcl_input_ps_siv linear noperspective v2.x, position
dcl_input_ps_siv constant v3.x, rendertarget_array_index
dcl_input_ps_siv constant v3.y, viewport_array_index
dcl_input_ps_sgv v3.z, primitive_id
dcl_input_ps_siv linear v4.x, cull_distance
dcl_input_ps_siv linear v4.y, clip_distance
dcl_input_ps_sgv v5.x, sampleIndex
dcl_input_ps_sgv v5.y, is_front_face
dcl_input vCoverage
dcl_output o0.xyzw
dcl_temps 13
dcl_indexableTemp x0[8], 4
dcl_indexableTemp x1[4], 4
dcl_indexableTemp x2[4], 4
ftou r0.x, v2.x
utof r0.x, r0.x
dp3 r0.y, v0.yzwy, l(1.000000, 2.000000, 3.000000, 0.000000)
add r0.x, r0.y, r0.x
add r0.x, r0.x, v1.x
xor r0.y, cb0[1].y, l(2)
itof r0.y, r0.y
add r0.x, r0.y, r0.x
mov r0.y, r0.x
mov r0.z, l(0)
loop 
  ige r0.w, r0.z, l(10)
  breakc_nz r0.w
  itof r0.w, r0.z
  add r1.x, r0.w, l(1.001000)
  div r1.x, l(1.000000, 1.000000, 1.000000, 1.000000), r1.x
  sqrt r1.y, r0.y
  mad r0.w, r0.w, r1.x, r1.y
  add r0.w, r0.w, r0.y
  lt r1.x, r0.w, l(0.000000)
  if_nz r1.x
    mov r0.y, r0.w
    break 
  endif 
  iadd r0.z, r0.z, l(1)
  mov r0.yz, r0.wwzw
endloop 
add r0.x, r0.y, l(27.000000)
utof r0.y, v3.x
add r0.x, r0.y, r0.x
add r0.x, r0.x, v4.x
utof r0.y, v3.y
add r0.x, r0.y, r0.x
or r0.y, v3.y, v5.x
utof r0.y, r0.y
add r0.x, r0.y, r0.x
utof r0.y, v5.x
add r0.x, r0.y, r0.x
add r0.y, r0.x, l(4.000000)
movc r0.x, v5.y, r0.y, r0.x
add r0.x, r0.x, v4.y
utof r0.y, vCoverage.x
add r0.x, r0.y, r0.x
utof r0.y, v3.z
add r0.x, r0.y, r0.x
ftoi r0.y, r0.x
ilt r0.w, r0.y, l(0)
if_nz r0.w
  xor r0.w, r0.y, l(50)
else 
  ilt r1.x, l(5), r0.y
  if_nz r1.x
    and r0.w, r0.y, l(2222)
  else 
    iadd r0.w, r0.y, -cb0[r0.y + 2].x
  endif 
endif 
add r0.y, cb0[0].y, cb0[0].x
add r0.y, r0.y, cb0[0].z
add r0.y, r0.y, cb0[0].w
add r0.x, r0.y, r0.x
sample_indexable(texture2d)(float,float,float,float) r0.y, l(0.125000, 5.000000, 0.000000, 0.000000), t0.yxzw, s0
add r0.x, r0.y, r0.x
sample_indexable(texture2d)(float,float,float,float) r0.y, l(0.777000, 1234.500000, 0.000000, 0.000000), t0.yxzw, s1
add r0.x, r0.y, r0.x
sample_indexable(texturecube)(float,float,float,float) r0.y, l(0.125000, 5.000000, 1.000000, 0.000000), t1.yxzw, s0
add r0.x, r0.y, r0.x
sample_indexable(texture3d)(float,float,float,float) r0.y, l(0.125000, 5.000000, 1.000000, 0.000000), t2.xzyw, s0
add r0.x, r0.y, r0.x
ldms_aoffimmi_indexable(1,1,0)(texture2dms)(float,float,float,float) r0.y, l(0, 5, 0, 0), t3.yxzw, l(0)
add r0.x, r0.y, r0.x
div r0.y, l(1.000000, 1.000000, 1.000000, 1.000000), r0.x
sample_b_indexable(texture2d)(float,float,float,float) r0.y, r0.xyxx, t0.xyzw, s0, l(-15.000000)
add r0.x, r0.y, r0.x
sample_indexable(texture2d)(float,float,float,float) r0.y, l(0.000000, 5.000000, 0.000000, 0.000000), t4.xzyw, s0
add r0.x, r0.y, r0.x
sample_indexable(texture2d)(float,float,float,float) r0.y, l(0.000000, 5.000000, 0.000000, 0.000000), t5.xzyw, s0
add r0.x, r0.y, r0.x
itof r0.yz, r0.wwzw
mov x1[3].x, r0.y
mov x2[2].x, r0.y
mov x1[2].x, r0.x
mov x2[1].x, r0.x
and r0.y, r0.w, l(0x00087e85)
itof r0.w, r0.y
mov x0[0].x, r0.w
mov x2[0].x, r0.w
iadd r1.x, r0.y, l(42)
itof r1.x, r1.x
mov x0[1].x, r1.x
mov x2[3].x, r1.x
ishr r0.y, r0.y, l(76)
itof r1.x, r0.y
mov x1[1].x, r1.x
mov x0[2].x, r1.x
ishl r0.y, r0.y, l(22)
mov x0[3].x, l(0)
mov x1[0].x, l(0)
mov x0[4].x, l(0)
mov x0[5].x, l(0)
xor r1.x, l(0), cb0[2].x
utof r1.y, r1.x
mov x0[6].x, r1.y
and r1.y, r1.x, l(127)
and r1.x, r1.x, cb0[r1.y + 2].x
utof r1.y, r1.x
iadd r1.xz, r1.xxxx, l(66, 0, 1, 0)
imul null, r1.x, cb0[r1.z + 2].x, cb0[r1.x + 2].y
itof r1.x, r1.x
add r0.x, r0.x, r1.x
add r1.x, r0.x, r1.y
mov x0[7].x, r1.x
add r0.z, r0.w, r0.z
ftoi r0.w, r0.z
and r0.w, r0.w, l(7)
mov r0.w, x0[r0.w + 0].x
itof r0.y, r0.y
add r0.y, r0.w, r0.y
ftoi r0.w, r0.y
and r0.w, r0.w, l(7)
mov r1.x, x1[r0.w + 0].x
add r0.z, r0.z, r1.x
mov r0.w, x2[r0.w + 0].x
round_z r0.z, r0.z
add r0.z, r0.w, r0.z
mul r2.z, r0.x, l(0.200000)
add r2.w, r1.y, l(0.500000)
round_z r2.xy, r0.zyzz
resinfo_indexable(texture2d)(float,float,float,float) r3.xyw, l(0), t0.xyzw
ftou r0.y, r3.y
utof r3.y, r0.y
samplepos r4.xy, rasterizer.xyxx, r3.y
mov r0.z, r3.w
mov r4.z, l(55566.199219)
countbits r5.xy, r3.xyxx
mov r6.z, r0.x
mov r6.w, r1.y
mov r6.xy, r2.xyxx
mov r0.y, l(0)
loop 
  ftoi r1.x, r6.x
  ige r1.x, r0.y, r1.x
  breakc_nz r1.x
  itof r7.x, r0.y
  mov r8.xyzw, r6.xyzw
  mov r1.x, l(0)
  loop 
    ftoi r1.z, r8.y
    ige r1.z, r1.x, r1.z
    breakc_nz r1.z
    itof r7.y, r1.x
    sample_d_indexable(texture2d)(float,float,float,float) r9.xyzw, r7.xyxx, t0.xyzw, s0, l(1.500000, 1.500000, 0.000000, 0.000000), l(4.100000, 4.100000, 0.000000, 0.000000)
    mov r7.yzw, r8.wwxy
    mov r1.z, l(0)
    loop 
      ftoi r1.w, r7.z
      ige r1.w, r1.z, r1.w
      breakc_nz r1.w
      itof r10.x, r1.z
      mov r11.xyz, r7.zwyz
      mov r1.w, l(0)
      loop 
        ftoi r4.w, r11.y
        ige r4.w, r1.w, r4.w
        breakc_nz r4.w
        itof r10.y, r1.w
        sample_d_indexable(texture2d)(float,float,float,float) r10.yzw, r10.xyxx, t0.zxyw, s0, l(1.500000, 1.500000, 0.000000, 0.000000), l(4.100000, 4.100000, 0.000000, 0.000000)
        add r11.xyz, r10.yzwy, r11.xyzx
        iadd r1.w, r1.w, l(1)
      endloop 
      mov r7.yzw, r11.zzxy
      iadd r1.z, r1.z, l(1)
    endloop 
    mad r1.z, r7.y, l(2.000000), l(4.400000)
    mul r0.w, r4.y, r1.z
    mul r3.zw, r0.zzzw, r4.xxxz
    countbits r5.zw, r3.zzzw
    utof r10.xyzw, r5.xyzw
    add r10.xyzw, r3.xyzw, r10.xyzw
    bfrev r12.xyzw, r10.xyzw
    utof r12.xyzw, r12.xyzw
    add r10.xyzw, r10.xyzw, r12.xyzw
    eq r0.w, r10.x, l(2.000000)
    if_nz r0.w
      abort 
    endif 
    mul r12.xyzw, r10.zzzz, l(4.000000, 4.000000, 3.000000, 3.000000)
    ge r12.xyzw, r12.xyzw, -r12.yyww
    movc r12.xyzw, r12.xyzw, l(4.000000,0.250000,3.000000,0.333333), l(-4.000000,-0.250000,-3.000000,-0.333333)
    mul r1.zw, r10.zzzz, r12.yyyw
    frc r1.zw, r1.zzzw
    mul r1.zw, r1.zzzw, r12.xxxz
    eq r1.zw, r1.zzzw, l(0.000000, 0.000000, 0.000000, 0.000000)
    if_nz r1.z
      printf "%0 is fine!", r10.z
    else 
      printf "%0 is not %1 fine!", r10.z, r10.x
    endif 
    if_nz r1.w
      errorf "%0 is invalid", r10.z
    endif 
    add r9.xyzw, r9.xyzw, r10.xyzw
    add r8.xyzw, r9.xyzw, r8.xyzw
    iadd r1.x, r1.x, l(1)
  endloop 
  mov r6.xyzw, r8.xyzw
  iadd r0.y, r0.y, l(1)
endloop 
mad r0.xyzw, r6.xyzw, l(2.000000, 2.000000, 2.000000, 2.000000), r2.xyzw
add o0.xyzw, r0.xyzw, l(1.100000, 2.200000, 3.300000, 4.400000)
ret 
// Approximately 210 instruction slots used
