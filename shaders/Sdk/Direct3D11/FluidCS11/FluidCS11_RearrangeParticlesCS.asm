//
// Generated by Microsoft (R) HLSL Shader Compiler 9.30.9200.16384
//
//
///
// Buffer Definitions: 
//
// Resource bind info for ParticlesRO
// {
//
//   struct Particle
//   {
//       
//       float2 position;               // Offset:    0
//       float2 velocity;               // Offset:    8
//
//   } $Element;                        // Offset:    0 Size:    16
//
// }
//
// Resource bind info for GridRO
// {
//
//   uint $Element;                     // Offset:    0 Size:     4
//
// }
//
// Resource bind info for ParticlesRW
// {
//
//   struct Particle
//   {
//       
//       float2 position;               // Offset:    0
//       float2 velocity;               // Offset:    8
//
//   } $Element;                        // Offset:    0 Size:    16
//
// }
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim Slot Elements
// ------------------------------ ---------- ------- ----------- ---- --------
// ParticlesRO                       texture  struct         r/o    0        1
// GridRO                            texture  struct         r/o    3        1
// ParticlesRW                           UAV  struct         r/w    0        1
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// no Input
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// no Output
cs_5_0
dcl_globalFlags refactoringAllowed
dcl_resource_structured t0, 16 
dcl_resource_structured t3, 4 
dcl_uav_structured u0, 16
dcl_input vThreadID.x
dcl_temps 1
dcl_thread_group 256, 1, 1
ld_structured_indexable(structured_buffer, stride=4)(mixed,mixed,mixed,mixed) r0.x, vThreadID.x, l(0), t3.xxxx
and r0.x, r0.x, l(0x0000ffff)
ld_structured_indexable(structured_buffer, stride=16)(mixed,mixed,mixed,mixed) r0.xyzw, r0.x, l(0), t0.xyzw
store_structured u0.xyzw, vThreadID.x, l(0), r0.xyzw
ret 
// Approximately 5 instruction slots used