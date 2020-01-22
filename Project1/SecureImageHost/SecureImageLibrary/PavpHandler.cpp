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
#include "PavpHandler.h"
#include <cstdio>
#include <string>
#include <comdef.h>
#include <fstream>
#include "crypt_data_gen.h"

using namespace std;

PavpHandler::PavpHandler() 
{ 
	m_PavpCreated = FALSE;

	m_pCryptoSession = NULL;
	m_pDecoderRenderTargets = NULL;
	m_pD3d9Device = NULL;
	m_pD3d9 = NULL;
	m_pD3D9VideoDevice=NULL;
}

PavpHandler* PavpHandler::Session()
{
	static PavpHandler instance;
	return &instance;
}

PavpHandler::~PavpHandler()
{
	m_PavpDevice.UninitializeDevice();
}

bool PavpHandler::ClosePAVPSession()
{
	bool ret = true;

	if (S_OK != DestroyPavp())
		ret = false;
	
	SAFE_RELEASE(m_pCryptoSession);
	SAFE_RELEASE(m_pDecoderRenderTargets);
	SAFE_RELEASE(m_pD3d9Device);
	SAFE_RELEASE(m_pD3d9);
	SAFE_RELEASE(m_pD3D9VideoDevice);
	
	return ret;
}

void PavpHandler::SetControlSize(UINT32 ControlWidth,UINT32 ControlHeight)
{
	DisplayHeight = ControlHeight;
	DisplayWidth = ControlWidth;
}

bool PavpHandler::Initialize(HWND targetControl)
{	
	image.targetControl = targetControl;
	#ifndef AMULET
	if (CreatePavp() != S_OK)
		return false;
	#endif	
	return true;
}

bool PavpHandler::GetMEhandle(unsigned int *pHandle)
{
	if (pHandle == NULL || m_PavpCreated == FALSE)
		return false;
	*pHandle = m_MeHandle;
	return true;
}

HRESULT PavpHandler::SetNewKey(UINT8* NewKey)
{
#ifndef AMULET
	//PAVP_SET_KEY_WYSIWYS: update Sn_d then enable heavy mode
	PavpEpidStatus stat=m_PavpDevice.SetNewKey(PAVP_SET_KEY_WYSIWYS,(StreamKey *)NewKey);
#else
	S1Kb = new UINT8[KEY_RECORD_SIZE];
	memcpy(S1Kb, NewKey, KEY_RECORD_SIZE);
#endif
	return S_OK;
}

HRESULT PavpHandler::InitializePavpDevice(E_PAVPAPI ePavpApi)
{
    HRESULT         hr = S_OK;
    PavpEpidStatus  PavpStatus = PAVP_STATUS_SUCCESS;

    if( NULL == m_pD3d9Device )
    {
        hr = CreateD3d9Device();
        if( FAILED(hr) )
        {
            return E_FAIL;
        }
    }

	PavpStatus = m_PavpDevice.InitializeDevice(m_pD3d9Device,m_pCryptoSession, ePavpApi);
    hr = PAVP_EPID_SUCCESS(PavpStatus) ? S_OK : E_FAIL;

    return hr;
}

HRESULT PavpHandler::CreateD3d9Device()
{
	HRESULT					hr = S_OK;
	D3DPRESENT_PARAMETERS	d3dPresParams;

	do
	{
        ZeroMemory(&d3dPresParams, sizeof(d3dPresParams));

		if( NULL != m_pD3d9Device )
		{
			SAFE_RELEASE(m_pD3d9Device);
		}

        if( NULL == m_pD3d9 )
        {
		    hr = Direct3DCreate9Ex(D3D_SDK_VERSION,&m_pD3d9);
		    if(NULL == m_pD3d9)
		    {
			    hr = E_FAIL;
			    break;
		    }
			D3DCAPS9 d3d9caps;
			hr=m_pD3d9->GetDeviceCaps(D3DADAPTER_DEFAULT,D3DDEVTYPE_HAL,&d3d9caps);
				if(!(d3d9caps.Caps & D3DCAPS_OVERLAY))
				{
					hr = E_FAIL;
			    break;
				}
        }

		D3DPRESENT_PARAMETERS d3dppex = {0};
		DWORD behaviorSwFlags = D3DCREATE_SOFTWARE_VERTEXPROCESSING | D3DCREATE_FPU_PRESERVE;

		d3dppex.BackBufferWidth = DisplayWidth;
		d3dppex.BackBufferHeight = DisplayHeight;
		d3dppex.BackBufferFormat = D3DFMT_X8R8G8B8;
		d3dppex.SwapEffect = D3DSWAPEFFECT_OVERLAY;
		d3dppex.hDeviceWindow = image.targetControl;
		d3dppex.Windowed = TRUE;
		d3dppex.Flags = D3DPRESENTFLAG_VIDEO;
		d3dppex.FullScreen_RefreshRateInHz = D3DPRESENT_RATE_DEFAULT;
		d3dppex.PresentationInterval       = D3DPRESENT_INTERVAL_ONE;
   
		// Create the D3DDevice
		hr = m_pD3d9->CreateDeviceEx(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL,
										   NULL, behaviorSwFlags,
										   &d3dppex, NULL,
										   &m_pD3d9Device);
	} while( FALSE );

	return hr;
}

HRESULT PavpHandler::DestroyPavp()
{
    PavpEpidStatus PavpStatus = m_PavpDevice.UninitializeDevice();
    return PAVP_EPID_SUCCESS(PavpStatus) ? S_OK : E_FAIL;
}

HRESULT PavpHandler::InitializeCryptoSession(PAVP_SESSION_TYPE eSessionType)
{
    HRESULT     hr;
    GUID        CryptoType = D3DCRYPTOTYPE_INTEL_AES128_CTR;
    GUID        DecodeProfile = DXVA2_ModeMPEG2_VLD;

    if( NULL == m_pD3d9Device )
    {
        hr = CreateD3d9Device();
        if( FAILED(hr) )
        {
            return S_OK;
        }
    }

	hr = m_pD3d9Device->QueryInterface(IID_IDirect3DDevice9Video,(void **)&m_pD3D9VideoDevice);
    if(FAILED(hr))
    {
        return E_FAIL;
    }
    hr = m_pD3D9VideoDevice->CreateCryptoSession(&CryptoType, 
                                                &DecodeProfile, 
                                                &m_pCryptoSession, 
                                                &m_hCryptoSession);
    
    if( FAILED (hr) )
    {
        return E_FAIL;
    }

    return hr;

}

HRESULT PavpHandler::CreatePavp()
{
    HRESULT         hr = S_OK;
    PavpEpidStatus PavpStatus = PAVP_STATUS_SUCCESS;
    PAVP_QUERY_CAPS2        sPavpCaps = {0};
	unsigned int Handle = 0;
	GetCapsOutBuff pchCaps = {0};
    do
    {
        m_ePavpApi = e_GpuCp;  
        DestroyPavp();
        if( FAILED(hr = InitializeCryptoSession(PAVP_SESSION_TYPE_DECODE)) )
        {
            hr = E_FAIL;
            break;
        }

        if( FAILED(hr = InitializePavpDevice(e_GpuCp)) )
        {
			hr = E_FAIL;
            break;
        }

        PavpStatus = m_PavpDevice.GetPavpCaps(&sPavpCaps);

		PavpStatus = m_PavpDevice.CreatePavpService(PAVP_KEY_EXCHANGE_DAA);   
		//using Intel DAL interface requires heavy mode
		m_PavpDevice.ForceHeavyModeEarly(PAVP_EPID_API_VERSION);
        if( PAVP_EPID_FAILURE(PavpStatus) )
        {
            hr = E_FAIL;
            break;
        }

		PavpStatus = m_PavpDevice.GetPchCaps(&pchCaps);
		if( PAVP_EPID_FAILURE(PavpStatus) )
        {
			hr = E_FAIL;
            break;
		}

		if ( (PavpStatus = m_PavpDevice.GetPavpHandle(&Handle)) != PAVP_STATUS_SUCCESS )
		{
			hr = E_FAIL;
			break;
		}
		else
		{
			m_MeHandle = Handle;
			m_PavpCreated = TRUE;
		}
    } while( FALSE );

    return hr;
}

bool PavpHandler::ShowImage(UINT8* BmpBuffer,UINT32 BmpBufferSize,HWND targetControl)
{
    HRESULT hr = S_OK;

	BOOL bUsePavp = true;
	static bool firstTime = true;

	image.BmpBuffer = BmpBuffer;
	image.BmpBufferSize = BmpBufferSize;

	if( FAILED(hr = DoDecryptionBlt()) )
	  return false; 

	if (!firstTime)
	{
		if (FAILED(hr = DisplayVideo()))
			return false;
	}
	else
		firstTime = false;

	return true;
}

HRESULT PavpHandler::DisplayVideo()
{  
#ifndef AMULET
    HRESULT             hr = S_OK;
    IDirect3DSurface9   *pD3dBackBuffer = NULL;
    do
    {
        if( NULL == m_pD3d9Device )
        {
            hr = E_UNEXPECTED;
            break;
        }
		
		hr = m_pD3d9Device->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &pD3dBackBuffer);
		
		if( FAILED(hr) )
		{
			break;
		}

        hr = m_pD3d9Device->StretchRect(m_pDecoderRenderTargets, NULL, pD3dBackBuffer, NULL, D3DTEXF_NONE);
        if( FAILED(hr) )
        {
            break;
        }

		SetWindowPosition();

		hr = m_pD3d9Device->PresentEx(NULL, NULL, image.targetControl , NULL,0x00000080L); 

    }   while( FALSE );

    SAFE_RELEASE(pD3dBackBuffer);

    return hr;
#else
	HRESULT             hr = S_OK;
	HDC hDC=NULL, memDC=NULL;

	BOOL bRet = FALSE;

	do {

	
		if( NULL == (hDC = ::GetDC( image.targetControl )) ) break;

		if( NULL == (memDC = ::CreateCompatibleDC ( hDC )) ) break;
	
		if( NULL == ::SelectObject( memDC, hBmp ) ) break;
		LONG w = DisplayWidth;
		LONG h = DisplayHeight;


		SetStretchBltMode(hDC,STRETCH_DELETESCANS);
		if( FALSE == (bRet = ::StretchBlt( hDC, 0, 0, 400, 300, memDC, 0, 0,w,h, SRCCOPY )) ) break;
	
	} while ( false );
		DWORD err=GetLastError();
	if( memDC != NULL ) ::DeleteDC( memDC );

	if( hDC != NULL) ::ReleaseDC( image.targetControl, hDC );

	if( !bRet ) hr = E_FAIL;

    return hr;
#endif
}

HRESULT PavpHandler::SetWindowPosition()
{
    HRESULT         hr = S_OK;
    PavpEpidStatus  PavpStatus = PAVP_STATUS_SUCCESS;
    WINDOWINFO      WindowInfo = {sizeof(WINDOWINFO)};
    MONITORINFO     MonitorInfo = {sizeof(MONITORINFO)};

    if( m_PavpDevice.GetDeviceState() != CAuxiliaryDevice::AccelServiceCreated )
    {
        return S_OK;
    }

    // Call the PAVP window position interface
    if (GetWindowInfo(image.targetControl,&WindowInfo) &&
		GetMonitorInfo(MonitorFromWindow(image.targetControl, MONITOR_DEFAULTTONULL), &MonitorInfo))
    {
        static RECT rWindowPosition = {0};
        RECT rIntersectRect;

        if (IntersectRect(&rIntersectRect, &WindowInfo.rcWindow, &MonitorInfo.rcMonitor))
        {
            if (0 != memcmp(&rWindowPosition, &rIntersectRect, sizeof(RECT)))
            {

				rWindowPosition = rIntersectRect;
				RECT rContent;

                rContent.left   = 0;
                rContent.top    = 0;
                rContent.right  = (LONG)DisplayWidth;
                rContent.bottom = (LONG)DisplayHeight;

                if (WindowInfo.rcWindow.left < MonitorInfo.rcMonitor.left)
                {
                    rContent.left = (LONG)((float) DisplayWidth *
                                    (float)(MonitorInfo.rcMonitor.left - WindowInfo.rcWindow.left) /
                                    (float)(WindowInfo.rcWindow.right - WindowInfo.rcWindow.left));
                }
                if (WindowInfo.rcWindow.top < MonitorInfo.rcMonitor.top)
                {
                    rContent.top = (LONG)((float)DisplayHeight *
                                   (float)(MonitorInfo.rcMonitor.top - WindowInfo.rcWindow.top) / 
                                   (float)(WindowInfo.rcWindow.bottom - WindowInfo.rcWindow.top));
                }
                
                if (WindowInfo.rcWindow.right > MonitorInfo.rcMonitor.right)
                {
                    rContent.right = (LONG)((float)(DisplayWidth) *
                                     (float)(MonitorInfo.rcMonitor.right - WindowInfo.rcWindow.left) /
                                     (float)(WindowInfo.rcWindow.right - WindowInfo.rcWindow.left));
                }
                if (WindowInfo.rcWindow.bottom > MonitorInfo.rcMonitor.bottom)
                {
                    rContent.bottom = (LONG)((float)DisplayHeight *
                                      (float)(MonitorInfo.rcMonitor.bottom - WindowInfo.rcWindow.top) /
                                      (float)(WindowInfo.rcWindow.bottom - WindowInfo.rcWindow.top));
                }
                
	            PavpStatus = m_PavpDevice.SetWindowPosition(rWindowPosition, rContent, image.targetControl);
                hr = PAVP_EPID_SUCCESS(PavpStatus) ? S_OK : E_FAIL;
            }
        }
    }
    return hr;
}

HRESULT PavpHandler::DoDecryptionBlt()
{
#ifndef AMULET

    HRESULT                 hr = E_FAIL;
    IDirect3DSurface9       *pEncryptedSrc = NULL;
    IDirect3DSurface9       *pDecryptedDst = NULL;
    BYTE                    *pSysMemEncrypted = NULL;
    D3DAES_CTR_IV           iv;
    UINT                    SysMemSize;
	DWORD                   dwCounter[4] = {0};
	UINT					SrcPitch, DXDstPitch;

    do
    {
		hr = GetContentProtectionCaps();
        if (FAILED(hr))
        {
            hr = E_FAIL;
            break;
        } 
        //Now create a regular surface so we can get the surface pitch and we will decrypt to it
        hr = m_pD3d9Device->CreateOffscreenPlainSurfaceEx(  DisplayWidth,
                                                            DisplayHeight,
															//we get 32 bit XRGB picture, 8 bits for each color
                                                            D3DFMT_X8R8G8B8,
                                                            D3DPOOL_DEFAULT, //This memory allocation consumes system RAM 
                                                            &pDecryptedDst,
                                                            NULL,  
                                                            D3DUSAGE_RESTRICTED_CONTENT);

        if( FAILED( hr ) ) {
            break;
        }
		
		//get the stride of the surface we created above
		hr = m_pCryptoSession->GetSurfacePitch(pDecryptedDst, &DXDstPitch);
		if (FAILED(hr)) {
			break;
		}
		//the memory size of the surface memory is the stride of the surface * height of the surface
        SysMemSize = DXDstPitch * DisplayHeight; 

		UINT nRemainBytes = SysMemSize%m_ContentProtectionCaps.BlockAlignmentSize;
		if( nRemainBytes > 0 ) 
		{
			//add padding to accomodate the values of BlockAlignmentSize
			SysMemSize = SysMemSize + ( m_ContentProtectionCaps.BlockAlignmentSize - nRemainBytes );
		}
		//allocate aligned memory to accomodate the values of BufferAlignmentStart
		pSysMemEncrypted = (BYTE *)_aligned_malloc(SysMemSize, m_ContentProtectionCaps.BufferAlignmentStart);

		//copy the encrypted image to the allocated buffer
		memcpy_s( pSysMemEncrypted, SysMemSize, image.BmpBuffer, SysMemSize);

		//create the source surface, and pass the memory aligned pointer of the encrypted data
		hr = m_pD3d9Device->CreateOffscreenPlainSurfaceEx(  DisplayWidth,
                                                            DisplayHeight,
															//we get 32 bit XRGB picture, 8 bits for each color
                                                            D3DFMT_X8R8G8B8,
                                                            D3DPOOL_SYSTEMMEM,  //This memory allocation consumes system RAM
                                                            &pEncryptedSrc,
                                                            (HANDLE*)&pSysMemEncrypted, //This parameter can be used in Direct3D 9 to share resources.
                                                            0);
        if (FAILED(hr)) {
            break;
        }

		//get the IV
		m_EncryptionMode.eCounterMode = PAVP_COUNTER_TYPE_A;
		GetNewCounter(dwCounter);
        CopyMemory((void*) &(iv.IV), dwCounter, sizeof(dwCounter));
		CPavpDevice::CounterEndianSwap((DWORD*) &(iv.IV), m_EncryptionMode.eCounterMode);

		//write the encrypted data to the detination - the protected surface
		hr = m_pCryptoSession->DecryptionBlt(/*src surface, contains source data*/pEncryptedSrc,
											/*destination surface, the data wil be written to*/pDecryptedDst,
											/*size of src surface*/ SysMemSize, 
											/*The entire surface is encrupted, sending NULL*/NULL, NULL, &iv);
		if (FAILED(hr)) {
            break;
        }
        m_pDecoderRenderTargets = pDecryptedDst;        

    } while( false );

	SAFE_RELEASE(pEncryptedSrc);

	if (pSysMemEncrypted != NULL) {
		_aligned_free(pSysMemEncrypted);
	}
    return hr;
#else
	string kbFilePath = getenv("DALsdk");
	kbFilePath.append("\\Platforms\\ME10.0\\Emulator\\PAVPKey.bin");
	unsigned char kb[KEY_RECORD_SIZE];
	unsigned char s1[KEY_RECORD_SIZE];
	UINT8 * decryptedImg = new UINT8[image.BmpBufferSize];	
	//read kb key from the updated file
	ifstream file(kbFilePath, ios::in|ios::binary|ios::beg);
	if (file.is_open())
	{
		file.read ((char*)kb, KEY_RECORD_SIZE);
		file.close();
	}		

	//decrypt s1 from s1(kb) using kb from file
	CdgStatus status=Aes128EcbDecrypt(S1Kb, KEY_RECORD_SIZE, s1, KEY_RECORD_SIZE, kb, KEY_RECORD_SIZE);
	if(status != CdgStsOk)
		return S_FALSE;

	//decrypt image from image(s1) using s1
	unsigned int dwCounter[4] = {0,0,0,1};

    DWORD temp   = dwCounter[2];
    dwCounter[2] = dwCounter[3];
    dwCounter[3] = temp;

    SwapEndian_8B(dwCounter);
    SwapEndian_8B(dwCounter+2); 

	status=Aes128CtrDecrypt(image.BmpBuffer, image.BmpBufferSize, decryptedImg, image.BmpBufferSize, s1,KEY_RECORD_SIZE, (unsigned int *)dwCounter, 32); 
	if(status != CdgStsOk)
		return S_FALSE;

    HRESULT hr = E_FAIL;

	hBmp = NULL;
	do {
		hBmp = ::CreateBitmap( DisplayWidth, DisplayHeight, 1, 32, decryptedImg );
		if( hBmp == NULL ) {
			break;
		}
		hr = S_OK;
	} while ( false );


	delete[] decryptedImg;
    return hr;
	
#endif
}

HRESULT PavpHandler::GetBitmapInfo(char *PathName, UINT &Height, UINT &Width)
{
    HBITMAP                 hBitmap;
    BITMAP                  Bitmap;

    //Load the bitmap first using the GDI to get info about it
    hBitmap = (HBITMAP)LoadImageA(   NULL, 
                                    PathName,
                                    IMAGE_BITMAP,
                                    0,
                                    0,
                                    LR_LOADFROMFILE | LR_CREATEDIBSECTION );
    if( hBitmap == NULL )
    {
        return E_FAIL;
    }

    //Get info about the object
    GetObjectA( hBitmap, sizeof( BITMAP ), &Bitmap );
    //Unload the bitmap from memory
    DeleteObject( hBitmap );

    Height = Bitmap.bmHeight;
    Width = Bitmap.bmWidth;
    return S_OK;
}

HRESULT PavpHandler::GetContentProtectionCaps(void)
{
    HRESULT hr;
    GUID pCryptoType = D3DCRYPTOTYPE_INTEL_AES128_CTR;
    GUID pDecodeProfile = DXVA2_ModeMPEG2_VLD;

    SAFE_RELEASE(m_pD3D9VideoDevice);

    hr = m_pD3d9Device->QueryInterface(IID_IDirect3DDevice9Video,(void **)&m_pD3D9VideoDevice);
    if(FAILED(hr))
    {
        return E_FAIL;
    }
    
    if (m_pD3D9VideoDevice == NULL)
    {
        return E_FAIL;

    }

    hr = m_pD3D9VideoDevice->GetContentProtectionCaps(&pCryptoType,&pDecodeProfile,&m_ContentProtectionCaps);
    if (FAILED(hr))   
    {
        return E_FAIL;
    }
    return S_OK;

}

void PavpHandler::GetNewCounter(DWORD dwCounter[4])
{
    switch(m_EncryptionMode.eCounterMode)
        {
        case PAVP_COUNTER_TYPE_C:
            dwCounter[0] = (rand() << 16) | (rand() & 0xffff);
            dwCounter[1] = (rand() << 16) | (rand() & 0xffff);
            dwCounter[2] = (rand() << 16) | (rand() & 0xffff);
            dwCounter[3] = (rand() << 16) | (rand() & 0xffff);
            break;
        case PAVP_COUNTER_TYPE_B:
            dwCounter[0] = 0;
            dwCounter[1] = (rand() << 16) | (rand() & 0xffff);
            dwCounter[2] = (rand() << 16) | (rand() & 0xffff);
            dwCounter[3] = (rand() << 16) | (rand() & 0xffff);
            break;
        case PAVP_COUNTER_TYPE_A:
            default:
            dwCounter[0] = 0;
            dwCounter[1] = 0;
            dwCounter[2] = 0;
            dwCounter[3] = 1;
            break;
        }
}