$(document).ready(function () {

    $('#btnPaste').click(async function () {
        try {
            showFormLoading();
            const fromClipboard = await navigator.clipboard.readText();
            
            const response = await fetch('/Paste/ProcessPaste', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(fromClipboard)
            });

            const result = await response.json();
            
            if (result.success) {
                console.log('Pasted text:', result.car);

                // Update the form with the received data
                Object.keys(result.car).forEach(key => {
                    $("#" + key).val(result.car[key]);
                });
             
            } else {
                alert('Error processing paste: ' + result.error);
            }

            hideFormLoading();
        } catch (error) {
            hideFormLoading();
            alert('Error accessing clipboard: ' + error.message);
        }
    });


    function showFormLoading() {
        document.getElementById('loadingOverlay').style.display = "block";
    }

    function hideFormLoading() {
        document.getElementById('loadingOverlay').style.display = "none";
    }

});
