mergeInto(LibraryManager.library, {
    ReadTxtFile: function(gameObjectNamePtr) {
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
        fileInput.setAttribute('accept','.txt');
        
        fileInput.onclick = function (event) {
            // File dialog opened
            this.value = null;
        };
        fileInput.onchange = function (event) {
            // File selected
            var fr = new FileReader();
            fr.onload = function() {
                SendMessage(gameObjectName, 'OnFileUploaded', fr.result);
            }
            fr.readAsText(event.target.files[0]);
        }
        document.body.appendChild(fileInput);

        document.onmouseup = function() {
            fileInput.click();
            document.onmouseup = null;
        }
    },

    DownloadTxtFile: function(fileNamePtr, fileContentPtr) {
        fileName = Pointer_stringify(fileNamePtr);
        fileContent = Pointer_stringify(fileContentPtr);

        var element = document.createElement('a');
        element.setAttribute('href','data:text/plain;charset=utf-8,' + encodeURIComponent(fileContent));
        element.setAttribute('download', fileName);
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    }
});
