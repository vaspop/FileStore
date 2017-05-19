Howto:

1. Get token http://[host]/api/token
Header => Content-Type: application/x-www-form-urlencoded
Body => username=secretname&password=secretstuff
Returns => [token]

2. Initiate file upload by providing file meta data. Needed in order to get a id for the file.

Header => Authorization: bearer [token]
HTTP post => http://[host]/api/files/InitiateFileUpload
Body => Json with the following properties; FileName, ContentType, Description 
Returns => [fileguid]

3. Upload file data http://[host]/api/files/UploadFileData/[fileguid]
Header => Authorization: bearer [token]

4. Download file metadata http://[host]/api/files/GetFileMetadata/[fileguid]
Header => Authorization: bearer [token]
Returns => Json with file metadata

4. Download file metadata http://[host]/api/files/DownloadFileData/[fileguid]
Header => Authorization: bearer [token]
Returns => File data