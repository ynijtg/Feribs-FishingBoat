# Feribs-FishingBoat
This tool automatizate fishing in World of Warcraft (32bit) by internal interactions to the game client.

# Preview
https://www.youtube.com/edit?o=U&video_id=c_SrzqFQoWk

# Info
Build of WoW 32bit 7.1.5.23420

# How it works
The tool allocates some memory for the code cave.
Then it will write the byte pattern into memory.
After that, all placeholders will be overwritten by the correct offsets/function addresses.

You might noticed we start our code cave with 0x90 (NOP).
This is due to the anti-cheat that blizzard uses, what they do is replace the first byte by 0xC3 (ret).
There is a bypass function in the code that will make our thread start at the second byte of the code cave.

# The Code Cave
```asm
0x55,                                                               //Push ebp
0x8B, 0xEC,                                                         //mov ebp,esp
0xB8, 0xDE, 0xAD, 0xBE, 0xEF,                                       //mov eax,DataMem
0xC7, 0x80, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,         //mov [eax+000000F8],00000000
0x50,                                                               //push eax
0x6A, 0x00,                                                         //push 00
0x8D, 0x58, 0x24,                                                   //lea ebx,[eax+24]
0x53,                                                               //push ebx
0x8D, 0x58, 0x14,                                                   //lea ebx,[eax+14]        
0x53,                                                               //push ebx
0x6A, 0x00,                                                         //push 00
0x8B, 0x58, 0x10,                                                   //mov ebx,[eax+10]
0x53,                                                               //push ebx
0x8B, 0x58, 0x04,                                                   //mov ebx,[eax+04]
0x8B, 0x1B,                                                         //mov ebx,[ebx]
0x53,                                                               //push ebx
0x8B, 0x18,                                                         //mov ebx,[eax]
0xFF, 0xD3,                                                         //call ebx
0x83, 0xC4, 0x18,                                                   //add esp,18
0x68, 0x40, 0x06, 0x00, 0x00,                                       //push 00000640
0xE8, 0xDE, 0xAD, 0xBE, 0xEF,                                       //call KERNEL32.Sleep       +0x36
0x58,                                                               //pop eax
0x8B, 0x58, 0x08,                                                   //mov ebx,[eax+08]
0x8B, 0x1B,                                                         //mov ebx,[ebx]
0x8B, 0x5B, 0x0C,                                                   //mov ebx,[ebx+0C]
0x8B, 0x5B, 0x44,                                                   //mov ebx,[ebx+44]
0x8B, 0xCB,                                                         //mov ecx,ebx
0x8B, 0x51, 0x10,                                                   //mov edx,[ecx+10]
0x81, 0xFA, 0x00, 0x02, 0x00, 0x00,                                 //cmp edx,00000200
0x0F, 0x87, 0x62, 0x00, 0x00, 0x00,                                 //ja halloc + 0xB9
0x83, 0xFA, 0x05,                                                   //edx,05
0x75, 0xE7,                                                         //jne halloc + 0x43
0x8B, 0x8B, 0x6C, 0x02, 0x00, 0x00,                                 //mov ecx,[ebx+0000026C]
0x8B, 0x89, 0xB4, 0x00, 0x00, 0x00,                                 //mov ecx,[ecx+000000B4]
0x8B, 0x09,                                                         //mov ecx,[ecx]
0x3B, 0x88, 0xFC, 0x00, 0x00, 0x00,                                 //cmp ecx,[eax+000000FC]
0x75, 0xD1,                                                         //jne halloc + 0x43

0x50,                                                               //push eax
0x8B, 0x8B, 0x28, 0x03, 0x00, 0x00,                                 //mov ecx,[ebx+00000328]
0x8B, 0x90, 0xF4, 0x00, 0x00, 0x00,                                 //edx,[eax+000000F4]
0x3B, 0x0A,                                                         //cmp ecx,[edx]
0x58,                                                               //pop eax
0x75, 0xBF,                                                         //jne halloc + 0x43

0xB9, 0x00, 0x00, 0x00, 0x00,                                       //mov ecx,00000000
0x41,                                                               //inc ecx        <--------------------------------------#
0x83, 0xF9, 0x42,                                                   //cmp ecx,42                                            |
0x0F, 0x84, 0x26, 0x00, 0x00, 0x00,                                 //je hAlloc + A9                Exit:                   |
0x50,                                                               //push eax                                              |
0x53,                                                               //push ebx                                              |
0x51,                                                               //push ecx                                              |
0x68, 0x60, 0x01, 0x00, 0x00,                                       //push 00000160                                         |
0xE8, 0xDE, 0xAD, 0xBE, 0xEF,                                       //call KERNEL32.Sleep         +0x9C                     |
0x59,                                                               //pop ecx                                               |
0x5B,                                                               //pop ebx                                               |
0x58,                                                               //pop eax                                               |
0x8B, 0x93, 0xF8, 0x00, 0x00, 0x00,                                 //mov edx,[ebx+000000F8]                                |
0x80, 0xFA, 0x01,                                                   //cmp dl,01                                             |
0x75, 0xDB,                                                         //jne --------------------------------------------------#

0x8B, 0xD3,                                                         //mov edx,ebx
0x83, 0xC2, 0x30,                                                   //add edx,30
0x52,                                                               //push edx
0x8B, 0x50, 0x0C,                                                   //mov edx,[eax+0C]
0xFF, 0xD2,                                                         //call edx
0x8B, 0xE5,                                                         //mov esp,ebp
0x5D,                                                               //pop ebp
0xC3,                                                               //Ret   
0xBE, 0x83, 0x54, 0x00,                                             ///Spell_C_CastSpell() func 
0x50, 0x99, 0x0D, 0x01,                                             ///PlayerBase Ptr 
0x5C, 0xD2, 0x03, 0x01,                                             ///ObjMgr Ptr 
0x02, 0x31, 0x2F, 0x00,                                             ///InteratObjByGUID() Func 
0x92, 0x01, 0x02, 0x00,                                             ///Spell id 
0x00, 0x00, 0x00, 0x00,                                             ///TargetGUID[4*DWORD]
```
