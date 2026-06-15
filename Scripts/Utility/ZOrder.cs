using SPHTypes;
using System;
using System.Numerics;
namespace Utility.ZOrder;
public static class ZOrder
{
    public static uint ZIndex2D(uint X, uint Y)
    {
        return spaceBitsByOne(X)|(spaceBitsByOne(Y)<<1);
    }

    public static uint ZIndex3D(uint X, uint Y, uint Z)
    {
        return (spaceBitsByTwo(Z)<<2)|(spaceBitsByTwo(Y)<<1)|spaceBitsByTwo(X);
    }

    private static uint spaceBitsByOne(uint num)
    {
        num &= 0x0000ffff;
        num = (num|(num<<8)) & 0b00000000111111110000000011111111;
        num = (num|(num<<4)) & 0b00001111000011110000111100001111;
        num = (num|(num<<2)) & 0b00110011001100110011001100110011;
        num = (num|(num<<1)) & 0b01010101010101010101010101010101;
        return num;
    }

    private static uint spaceBitsByTwo(uint num)
    {                      //indices:   10 9  8  7  6  5  4  3  2  1  
        num &= 0x0000ffff;//say we have 1  1  1  1  1  1  1  1  1  1. How far does each num have to move?
        //                              18 16 14 12 10 8  6  4  2  0
        num = (num|(num<<16))& 0b00000011000000000000000011111111; //move 10 to 26, 9 to 25
        num = (num|(num<<8)) & 0b00000011000000001111000000001111; //move 8 to 16, 7 to 15, 6 to 14, 5 to 13
        num = (num|(num<<4)) & 0b00000011000011000011000011000011; //move 16 to 20, 15 to 19, 4 to 8, 3 to 7
        num = (num|(num<<2)) & 0b00001001001001001001001001001001; //move 26 to 28, 20 to 22, 14 to 16, 8 to 10, 2 to 4
        return num;
    }
}