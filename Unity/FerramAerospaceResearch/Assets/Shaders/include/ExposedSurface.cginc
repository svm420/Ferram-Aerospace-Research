static const uint ByteMask = 0xFF;

inline int get_index(uint value)
{
    return value - 1;
}
uint pack_uint(float4 c)
{
    c *= 255;
    // no builtin char-types...
    uint4 u = c.abgr; // little-endian
    u &= ByteMask;
    u <<= int4(24, 16, 8, 0);
    return u.x | u.y | u.z | u.w;
}
