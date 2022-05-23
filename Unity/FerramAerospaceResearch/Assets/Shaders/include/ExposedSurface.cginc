static const uint TagLength = 10;
static const uint TagMask = (1 << TagLength) - 1;
static const uint ByteMask = 0xFF;

inline uint get_tag(uint value)
{
    return value & TagMask;
}

inline int get_index(uint value)
{
    return value >> TagLength;
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
