========================================================================
    Copyright (c) 2013 - 2019 Intel Corporation.
Intel(R) Dynamic Application Loader SDK: SIGMA 1.1 Sample Overview
========================================================================

Intel(R) DAL SDK has created this SIGMA Sample for you as a starting point.

Note: This sample is applicable for Intel(R) DAL API level 4 and above.

This sample demonstrates how to enable an application running on the firmware securely to exchange
symmetric keys with a remote party using Intel(R) DAL.


The SIGMA protocol sets up a secure session between an EPID prover and verifier.

A verifier can refer to different entities, such as a TRS-based host application, 
an Intel server or an ISV key provisioning server.
The prover is typically Intel hardware.
The SIGMA protocol is based on the Diffie-Hellman key exchange and uses the 
Intel EPID signing algorithm to authenticate the firmware to the remote party.
Since the SIGMA protocol uses an EPID signature, Intel EPID must be provisioned prior to using this class.


Sample Flow
=====================

1. The trusted application generates an S1 message which is transfered to the server by the host application.
	The S1 message contains:
		- Public part of the Diffie-Hellman key (g^a), generated for the current SIGMA session.
		- EPID group ID of this platform
		- OCSP request generated for this session
2. The server processes the S1 message.
3. The server derives SMK, SK and MK.
	SMK: Session Message Key - Derived from g^ab to verify SIGMA messages and used to derive SK and MK.
	MK:  Session Integrity Key - 128bit key derived from SMK and used for creating ICVs for each message passed 
				between the verifier and the prover.
	SK:  Session Confidentiality Key - 128bit key derived from SMK and used for encrypting messages between 
				the verifier and prover.
4. The server generates an S2 message which is transferred to the trusted application by the host application.
	The S2 message contains:
		- Signature using the verifier's private key on (g^a || g^b)
		- HMAC computed using the session MAC key on several fields in the message, as required by the protocol
		- Verifier's public part of the Diffie-Hellman key (g^b)
		- The BaseName chosen by the verifier (will be a part of the EPID signature in S3 message), or 0x00 for random based signatures.
		- OCSP response for the verifier's certificate chain
		- Verifier's certificate chain
		- The Signature Revocation List (SIG-RL) for the Group ID of the prover that was specified in S1 message		
5. The trusted application processes and verifies the S2 message.
6. The trusted application generates an S3 message which is then transferred to the server by the host application.
	The S3 message contains:
		- HMAC computed using the session MAC key on several fields in the message, as required by the protocol.
		- Task information that identifies the initiator of this SIGMA session inside the firmware, 
			including the specific trusted application that created this session according to the UUID
		- Prover's public part of the Diffie-Hellman key (g^a), identical to the one sent in S1.
		- Prover's EPID certificate
		- Prover's EPID signature on several fields in the message, as required by the protocol
		- Non-revoked proofs created by the prover, based on the Signature Revocation List from S2.  	
7. The server processes and verifies the S3 message.

=========================================================================================

When the SIGMA flow is finished, both parties have one shared secret and can use any symmetrical encryption algorithm to transfer sensitive data.