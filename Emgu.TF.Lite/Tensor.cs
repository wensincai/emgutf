﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Types supported by tensor
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// No type
        /// </summary>
        NoType = 0,
        /// <summary>
        /// single precision float
        /// </summary>
        Float32 = 1,
        /// <summary>
        /// Int32
        /// </summary>
        Int32 = 2,
        /// <summary>
        /// UInt8
        /// </summary>
        UInt8 = 3,
        /// <summary>
        /// Int64
        /// </summary>
        Int64 = 4,
        /// <summary>
        /// String
        /// </summary>
        String = 5,
    }

    /// <summary>
    /// Parameters for asymmetric quantization.
    /// </summary>
    /// <remarks>
    /// Quantized values can be converted back to float using:
    ///    real_value = scale * (quantized_value - zero_point);
    /// </remarks>
    public struct QuantizationParams
    {
        /// <summary>
        /// The scale
        /// </summary>
        float Scale;
        /// <summary>
        /// The zero point
        /// </summary>
        Int32 ZeroPoint;
    }

    /// <summary>
    /// Memory allocation strategies.
    /// </summary>
    public enum AllocationType
    {
        /// <summary>
        /// None
        /// </summary>
        MemNone = 0,
        /// <summary>
        ///  Read-only memory-mapped data (or data externally allocated).
        /// </summary>
        MmapRo,
        /// <summary>
        /// Arena allocated data
        /// </summary>
        ArenaRw,
        /// <summary>
        /// Arena allocated persistent data
        /// </summary>
        ArenaRwPersistent,
        /// <summary>
        /// Tensors that are allocated during evaluation
        /// </summary>
        Dynamic,
    }

    /// <summary>
    /// A tensorflow lite tensor
    /// </summary>
    public class Tensor : Emgu.TF.Util.UnmanagedObject
    {
        private readonly bool _needDispose;

        /// <summary>
        /// Create a Tensor from the native tensorflow lite tensor pointer
        /// </summary>
        /// <param name="ptr">A native tensorflow lite tensor pointer</param>
        /// <param name="needDispose">If true, we need to dispose the tensor upon object disposal. If false, we assume the tensor will be freed by the parent object.</param>
        public Tensor(IntPtr ptr, bool needDispose)
        {
            _ptr = ptr;
            _needDispose = needDispose;
        }

        /// <summary>
        /// The data type specification for data stored in `data`. This affects
        /// what member of `data` union should be used.
        /// </summary>
        public DataType Type
        {
            get
            {
                return TfLiteInvoke.tfeTensorGetType(_ptr);
            }
        }

        /// <summary>
        /// A raw data pointers. 
        /// </summary>
        public IntPtr DataPointer
        {
            get
            {
                return TfLiteInvoke.tfeTensorGetData(_ptr);
            }
        }

        /// <summary>
        /// Quantization information.
        /// </summary>
        public QuantizationParams QuantizationParams
        {
            get
            {
                QuantizationParams p = new QuantizationParams();
                TfLiteInvoke.tfeTensorGetQuantizationParams(_ptr, ref p);
                return p;
            }
        }

        /// <summary>
        /// How memory is mapped
        ///  kTfLiteMmapRo: Memory mapped read only.
        ///  i.e. weights
        ///  kTfLiteArenaRw: Arena allocated read write memory
        ///  (i.e. temporaries, outputs).
        /// </summary>
        public AllocationType AllocationType
        {
            get
            {
                return TfLiteInvoke.tfeTensorGetAllocationType(_ptr);
            }
        }

        /// <summary>
        /// The number of bytes required to store the data of this Tensor. I.e.
        /// (bytes of each element) * dims[0] * ... * dims[n-1].  For example, if
        /// type is kTfLiteFloat32 and dims = {3, 2} then
        /// bytes = sizeof(float) * 3 * 2 = 4 * 3 * 2 = 24.
        /// </summary>
        public int ByteSize
        {
            get
            {
                return TfLiteInvoke.tfeTensorGetByteSize(_ptr);
            }
        }

        /*
        /// <summary>
        /// An opaque pointer to a tflite::MMapAllocation
        /// </summary>
        public IntPtr Allocation
        {
            get
            {
                return TfLiteInvoke.tf
            }
        }*/

        /// <summary>
        /// Name of this tensor.
        /// </summary>
        public String Name
        {
            get
            {
                return Marshal.PtrToStringAnsi(TfLiteInvoke.tfeTensorGetName(_ptr));
            }
        }

        /// <summary>
        /// Get a copy of the tensor data as a managed array
        /// </summary>
        /// <returns>A copy of the tensor data as a managed array</returns>
        public Array GetData()
        {
            DataType type = this.Type;
            Type t = TfLiteInvoke.GetNativeType(type);
            if (t == null)
                return null;

            int byteSize = ByteSize;
            Array array;

            int len = byteSize / Marshal.SizeOf(t);
            array = Array.CreateInstance(t, len);

            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            TfLiteInvoke.tfeMemcpy(handle.AddrOfPinnedObject(), DataPointer, byteSize);
            handle.Free();
            return array;
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (_needDispose && IntPtr.Zero != _ptr)
            {
                //This should not be called, _needDisposed should always be false in the constructor, it should be managed by the interpretor.
                throw new Exception("_needDispose == true is not supported in the constructor");
            }
        }
    }


    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern DataType tfeTensorGetType(IntPtr tensor);


        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeTensorGetData(IntPtr tensor);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeTensorGetQuantizationParams(IntPtr tensor, ref QuantizationParams p);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern AllocationType tfeTensorGetAllocationType(IntPtr tensor);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int tfeTensorGetByteSize(IntPtr tensor);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeTensorGetName(IntPtr tensor);


        /// <summary>
        /// Get the equivalent native type from Tensorflow DataType
        /// </summary>
        /// <param name="dataType">The tensorflow DataType</param>
        /// <returns>The equivalent native type</returns>
        public static Type GetNativeType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Float32:
                    return typeof(float);
                case DataType.Int32:
                    return typeof(int);
                case DataType.UInt8:
                    return typeof(byte);
                case DataType.Int64:
                    return typeof(Int64);
                case DataType.String:
                    return typeof(Byte);
                default:
                    return null;
            }
        }
    }
}
