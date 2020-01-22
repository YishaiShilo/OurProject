/**
***
*** Copyright  (C) 2014 Intel Corporation. All rights reserved.
***
*** The information and source code contained herein is the exclusive
*** property of Intel Corporation. and may not be disclosed, examined
*** or reproduced in whole or in part without explicit written authorization
*** from the company.
***
*** ----------------------------------------------------------------------------
**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DALSamplesServer
{
    public enum CdgStatus
    {
        CdgStsOk = 0,					// No errors

        CdgStsNotInit,					// Library is not initialized
        CdgStsBadPtr,					// Bad pointer error

        CdgStsIntErr,					// Internal error (SafeId lib)

        CdgStsBuffTooSmall,				// Buffer specified for serialized key/certificate is too small
        CdgStsSerErr,					// Key/certificate serialization error
        CdgStsHashErr,					// Error computing hash

        CdgStsCryptCtxInitErr,			// Error initializing cryptosystem context
        CdgStsKeyPairGenErr,			// Error generating EC-DSA key pair
        CdqStsKeyPairVerErr,			// Error verifing EC-DSA key pair
        CdrStsKeyPairInv,				// Generated EC-DSA key pair is invalid
        CdgStsKeyPairSetErr,			// Error setting EC-DSA key pair in ECC context

        CdgStsSignErr,					// Error signing the message
        CdgStsVerifErr,					// Error verifying the signature

        CdgStsUndefined = 0xFF			// None of above, initial state

    }


    public enum CdgResult
    {
        CdgValid = 0,
        CdgInvalid
    }

    class CryptoDataGenWrapper_1_1
    {
        private const string CryptoDataGen_1_1_dll = "CryptoDataGen_1_1.dll";

        [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "MessageVerifyPch", CallingConvention = CallingConvention.Cdecl)]
        public static extern CdgStatus MessageVerifyPch(
            byte[] PubKeyPch,
            int PubKeyPchLen,
            byte[] EPIDParamsCert,
            byte[] Message,
            int MessageLen,
            byte[] Bsn,
            int BsnLen,
            byte[] Signature,
            int SignatureLen,
            out CdgResult VerifRes,
            byte[] privateKeyRevList,
            byte[] SignatureRevList = null,
            byte[] GroupRevList = null);

        [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "DeriveSigmaKeys", CallingConvention = CallingConvention.Cdecl)]
        public static extern CdgStatus DeriveSigmaKeys(
            byte[] Ga,
            int GaLen,
            byte[] Gb,
            int GbLen,
            byte[] Sk,
            int SkLen,
            byte[] Mk,
            int MkLen,
            byte[] SMK,
            int SMKLen);
    


    [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "CreateHmac", CallingConvention = CallingConvention.Cdecl)]
        public static extern CdgStatus CreateHmac(
            byte[] Message,
            int MessageLen,
            byte[] Mk,
            int MkLen,
            byte[] Hmac,
            int HmacLen);

    [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "MessageSign", CallingConvention = CallingConvention.Cdecl)]
    public static extern CdgStatus MessageSign(
        byte[] PrivKey,
        int PrivKeyLen,
        byte[] Message,
        int MessageLen,
        byte[] Signature,
        int SignatureLen);

    [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "GenerateSignatureRevList", CallingConvention = CallingConvention.Cdecl)]
    public static extern CdgStatus GenerateSignatureRevList(
        int GroupId,
        byte[] BKList,
        int BKListLen,
        byte[] SignatureRevList,
        int SignatureRevListLen,
        byte[] intelSignKey,
        int intelSignKeyLen,
        int noOfBKListItems = 0);

    [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "GenerateNewEPIDCertificate", CallingConvention = CallingConvention.Cdecl)]
    public static extern CdgStatus GenerateNewEPIDGroupCertificate(
        byte[] Gid,
        byte[] EPIDCertParameters,
        byte[] newEPIDCertificate,
        int CertificateLen,
        byte[] newPrivateKey,
        int PrivateKeyLen,
        byte[] intelSignKey,
        int intelSignKeyLen,
        int keyCount);

    [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "MessageSignPch", CallingConvention = CallingConvention.Cdecl)]
    public static extern CdgStatus MessageSignPch(
        byte[] CertPch,
        int CertPchWithoutSignatureLen,
        byte[] SignKey,
        int SignKeyLen,
        byte[] EPIDParamsCert,
        byte[] Message,
        int MessageLen,
        byte[] BaseName,
        int BaseNameLen,
        byte[] Signature,
        int SignatureLen,
        byte[] SigRevList);

    

    [DllImport(CryptoDataGen_1_1_dll, EntryPoint = "VerifyHmac", CallingConvention = CallingConvention.Cdecl)]
        public static extern CdgStatus VerifyHmac(
            byte[] Message,
            int MessageLen,
            byte[] Hmac,
            int HmacLen,
            byte[] MacKey,
            int MacKeyLen,
            ref CdgResult VerifRes);
    }
}
