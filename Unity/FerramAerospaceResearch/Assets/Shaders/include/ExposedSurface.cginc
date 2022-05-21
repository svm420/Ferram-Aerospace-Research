static const uint TagLength = 10;
static const uint TagMask = (1 << TagLength) - 1;

inline uint get_tag(const uint value)
{
    return value & TagMask;
}

inline int get_index(const uint value)
{
    return value >> TagLength;
}

uint pack_uint(float4 c)
{
    const uint mask = 0xFF;
    c *= 255;
    // no builtin char-types...
    uint4 u = c.abgr; // little-endian
    u &= mask;
    u <<= int4(24, 16, 8, 0);
    return u.x | u.y | u.z | u.w;
}
