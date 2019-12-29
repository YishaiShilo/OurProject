/**
***
*** Copyright  (C) 1985-2015 Intel Corporation. All rights reserved.
***
*** The information and source code contained herein is the exclusive
*** property of Intel Corporation. and may not be disclosed, examined
*** or reproduced in whole or in part without explicit written authorization
*** from the company.
***
*** ----------------------------------------------------------------------------
**/
#ifndef _PAVP_HANDLER_H
#define _PAVP_HANDLER_H

#include <Windows.h>
#include <d3d9.h>
#include <dxva.h>
#include <intel_pavp_device.h>
#include "intel_pavp_api.h"
#include "d3d9types.h"

#define KEY_RECORD_SIZE 16
const D3DCOLOR BLACK = D3DCOLOR_XRGB(0x00,0x00,0x00);

typedef struct _ImageData
{
	UINT8* BmpBuffer;
	UINT32 BmpBufferSize;
	HWND targetControl;
} ImageData;

class PavpHandler
{
public:
	
	static PavpHandler* Session();
	void SetControlSize(UINT32 ControlWidth,UINT32 ControlHeight);
	bool Initialize(HWND targetControl);
	bool GetMEhandle(unsigned int *pHandle);
	HRESULT SetNewKey(UINT8* NewKey);
	bool ShowImage(UINT8* BmpBuffer,UINT32 BmpBufferSize,HWND targetControl);
	HRESULT DisplayVideo();
	bool ClosePAVPSession();
	~PavpHandler();

private:
	PavpHandler();
	LONG DisplayHeight;
	LONG DisplayWidth;
	UINT8* S1Kb;
	HBITMAP hBmp;
	
	IDirect3DDevice9Ex	*m_pD3d9Device;
	CPavpDevice			m_PavpDevice;
	IDirect3D9Ex			*m_pD3d9;
	E_PAVPAPI           m_ePavpApi;

	IDirect3DCryptoSession9 *m_pCryptoSession;
	HANDLE                  m_hCryptoSession;
	IDirect3DDevice9Video   *m_pD3D9VideoDevice;
	D3DCONTENTPROTECTIONCAPS	m_ContentProtectionCaps;
	PAVP_ENCRYPTION_MODE        m_EncryptionMode;

	unsigned int        m_MeHandle;
	BOOL                m_PavpCreated;

	ImageData image;

	IDirect3DSurface9           *m_pDecoderRenderTargets;

	HRESULT InitializePavpDevice(E_PAVPAPI ePavpApi);
	HRESULT InitializeCryptoSession(PAVP_SESSION_TYPE eSessionType);
	HRESULT CreateD3d9Device();
	HRESULT CreatePavp();

	HRESULT SetWindowPosition();
	HRESULT DoDecryptionBlt();
	HRESULT GetBitmapInfo(char *PathName, UINT &Height, UINT &Width);
	HRESULT GetContentProtectionCaps(void);
	void GetNewCounter(DWORD dwCounter[4]);

	HRESULT DestroyPavp();

};

#endif