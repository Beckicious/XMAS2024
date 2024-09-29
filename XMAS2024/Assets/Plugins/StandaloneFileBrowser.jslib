var StandaloneFileBrowserWebGLPlugin = {
    // Open file.
    
	// gameObjectNamePtr: GameObject name required for calling back unity side with SendMessage. And it should be unique
   
    UploadJson: function(gameObjectNamePtr) {
        gameObjectName = Pointer_stringify(gameObjectNamePtr);

        // Delete if element exist
        var fileInput = document.getElementById(gameObjectName)
        if (fileInput) {
            document.body.removeChild(fileInput);
        }

        fileInput = document.createElement('input');
        fileInput.setAttribute('id', gameObjectName);
        fileInput.setAttribute('type', 'file');
        fileInput.setAttribute('style','display:none;');
        fileInput.setAttribute('style','visibility:hidden;');
        fileInput.setAttribute('accept','.json');
        
        fileInput.onclick = function (event) {
            // File dialog opened
            this.value = null;
        };
        fileInput.onchange = function (event) {
            // File selected
            SendMessage(gameObjectName, 'OnFileUploaded', URL.createObjectURL(event.target.files[0]) + "~" + event.target.files[0].name);
        }
        document.body.appendChild(fileInput);

        document.onmouseup = function() {
            fileInput.click();
            document.onmouseup = null;
        }
    },

    // Open folder. - NOT IMPLEMENTED
    UploadFolder: function(gameObjectNamePtr) {
        gameObjectName = Pointer_stringify(gameObjectNamePtr);
        SendMessage(gameObjectName, 'OnFolderUploaded', '');
    },

    // Save file
    // DownloadFile method does not open SaveFileDialog like standalone builds, its just allows user to download file
    // gameObjectNamePtr: GameObject name required for calling back unity side with SendMessage. And it should be unique
    //     DownloadFile does not return any info, just calls 'OnFileDownloaded' without any parameter
    // filenamePtr: Filename with extension
    // byteArray: byte[]
    // byteArraySize: byte[].Length
    DownloadFile: function(gameObjectNamePtr, filenamePtr, byteArray, byteArraySize) {
        gameObjectName = Pointer_stringify(gameObjectNamePtr);
        filename = Pointer_stringify(filenamePtr);

        var bytes = new Uint8Array(byteArraySize);
        for (var i = 0; i < byteArraySize; i++) {
            bytes[i] = HEAPU8[byteArray + i];
        }

        var downloader = window.document.createElement('a');
        downloader.setAttribute('id', gameObjectName);
        downloader.href = window.URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' }));
        downloader.download = filename;
        document.body.appendChild(downloader);

        document.onmouseup = function() {
            downloader.click();
            document.body.removeChild(downloader);
        	document.onmouseup = null;

            SendMessage(gameObjectName, 'OnFileDownloaded');
        }
    }
};

mergeInto(LibraryManager.library, StandaloneFileBrowserWebGLPlugin);