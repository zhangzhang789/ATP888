using System;
using System.IO;

namespace Package
{
    class DCPackage     //将所有定义的属性都定义在这个类下面，方便调用，而不是结构体
    {


        UInt16 cycle_;
        public UInt16 Cycle { set { cycle_ = value; } }

        UInt16 type_;
        public UInt16 PackageType { set { type_ = value; } }

        UInt16 length_;
        public UInt16 Length { set { length_ = value; } }

        UInt16 highSpeed_;
        public UInt16 HighSpeed { set { highSpeed_ = value; } }

        UInt16 openSpeed_;
        public UInt16 OpenSpeed { set { openSpeed_ = value; } }

        UInt16 permitSpeed_;
        public UInt16 PermitSpeed { set { permitSpeed_ = value; } }

        UInt16 interSpeed_;
        public UInt16 InterSpeed { set { interSpeed_ = value; } }

        UInt16 direction_;
        public UInt16 Direction { set { direction_ = value; } }

        Byte isEB_;
        public Byte IsEB { set { isEB_ = value; } }

        UInt16 nextSpeed_;
        public UInt16 NextSpeed { set { nextSpeed_ = value; } }




        public int DCPackStream(byte[] DCSendData)
        {

            using (Stream sendStream = new MemoryStream(DCSendData))
            using (BinaryWriter DCPackageStream = new BinaryWriter(sendStream))
            {
                DCPackageStream.Write((UInt16)0);
                DCPackageStream.Write(type_);
                DCPackageStream.Write(length_);
                DCPackageStream.Write(highSpeed_);
                DCPackageStream.Write(openSpeed_);
                DCPackageStream.Write(permitSpeed_);
                DCPackageStream.Write(interSpeed_);
                DCPackageStream.Write(direction_);
                DCPackageStream.Write(isEB_);
                DCPackageStream.Write(nextSpeed_);
                return (int)DCPackageStream.BaseStream.Position;
            }


        
        }
    }
}
