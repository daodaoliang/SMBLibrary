/* Copyright (C) 2014-2017 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using SMBLibrary.RPC;
using SMBLibrary.SMB1;
using SMBLibrary.Services;
using Utilities;

namespace SMBLibrary.Server.SMB1
{
    internal class TransactionSubcommandHelper
    {
        internal static TransactionTransactNamedPipeResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, TransactionTransactNamedPipeRequest subcommand, ISMBShare share, SMB1ConnectionState state)
        {
            SMB1Session session = state.GetSession(header.UID);
            OpenFileObject openFile = session.GetOpenFileObject(subcommand.FID);
            if (openFile == null)
            {
                header.Status = NTStatus.STATUS_INVALID_HANDLE;
                return null;
            }

            int maxOutputLength = (int)maxDataCount;
            byte[] output;
            header.Status = share.FileStore.DeviceIOControl(openFile.Handle, (uint)IoControlCode.FSCTL_PIPE_TRANSCEIVE, subcommand.WriteData, out output, maxOutputLength);
            if (header.Status != NTStatus.STATUS_SUCCESS && header.Status != NTStatus.STATUS_BUFFER_OVERFLOW)
            {
                return null;
            }
            TransactionTransactNamedPipeResponse response = new TransactionTransactNamedPipeResponse();
            response.ReadData = output;
            return response;
        }
    }
}
