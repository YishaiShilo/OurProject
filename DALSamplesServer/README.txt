===============================================================================
    Copyright (c) 2013 - 2019 Intel Corporation.
Intel(R) Dynamic Application Loader (Intel(R) DAL) SDK: Samples Server
===============================================================================

The Samples Server is a simple server for use with those samples that require one. The following samples require a server in order to run:

* Intel® EPID 1.1 Provisioning Sample
* Intel® EPID 1.1 Signing Sample
* SIGMA 1.1 Sample

When the host sample initiates communication with the samples server, it sends the sample server a code that tells the server which sample is connected.
The server then starts the specific flow for the sample, returns the requested data, and then listens for additional messages.

For details of how to run and use the server, see the page on the Samples Server in  the Intel(R) DAL Developer Guide.
