$(document).ready(function () {
    let debounceTimer;
    const textarea = document.getElementById('autoCompleteTextArea');
    const suggestionToggle = document.getElementById('suggestionToggle');
    let currentSuggestion = '';
    let originalValue = '';

    textarea.addEventListener('keydown', function (e) {
        clearTimeout(debounceTimer);

        if (e.key === 'Tab' && currentSuggestion) {
            e.preventDefault();
            acceptSuggestion();
        }
        else {
            debounceTimer = setTimeout(getSuggestion, 500);
        }
    });

    textarea.addEventListener('blur', function () {
        clearTimeout(debounceTimer);

        if (currentSuggestion) {
            clearSuggestion();
        }
    });

    async function getSuggestion() {
        const cursorPosition = textarea.selectionStart;
        const textBefore = textarea.value.substring(0, cursorPosition);
        const textAfter = textarea.value.substring(cursorPosition);

        try {
            const response = await fetch('/Suggestion/Get', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    textBefore: textBefore,
                    textAfter: textAfter
                })
            });

            const data = await response.json();
            if (data.success && data.suggestion) {
                showSuggestion(data.suggestion);
            } else {
                clearSuggestion();
            }
        } catch (error) {
            console.error('Error getting suggestion:', error);
            clearSuggestion();
        }
    }

    function showSuggestion(suggestion) {

        const cursorPosition = textarea.selectionStart;
        originalValue = textarea.value;
        currentSuggestion = suggestion;

        // Create a temporary div to measure the suggestion text
        const tempDiv = document.createElement('div');
        tempDiv.style.visibility = 'hidden';
        tempDiv.style.position = 'absolute';
        tempDiv.style.whiteSpace = 'pre-wrap';
        tempDiv.style.font = window.getComputedStyle(textarea).font;
        tempDiv.textContent = suggestion;
        document.body.appendChild(tempDiv);

        // Get the width of the suggestion text
        const suggestionWidth = tempDiv.offsetWidth;
        document.body.removeChild(tempDiv);

        // Update textarea with suggestion
        const textBefore = textarea.value.substring(0, cursorPosition);
        const textAfter = textarea.value.substring(cursorPosition);
        textarea.value = textBefore + suggestion + textAfter;

        // Create a style element for the suggestion
        const styleId = 'suggestion-style';
        let styleElement = document.getElementById(styleId);
        if (!styleElement) {
            styleElement = document.createElement('style');
            styleElement.id = styleId;
            document.head.appendChild(styleElement);
        }

        // Add CSS to style the suggestion text
        styleElement.textContent = `
        #autoCompleteTextArea {
            background: linear-gradient(to right,
        transparent ${cursorPosition}ch,
        #999 ${cursorPosition}ch,
        #999 ${cursorPosition + suggestion.length}ch,
        transparent ${cursorPosition + suggestion.length}ch
        );
        background-clip: text;
        -webkit-background-clip: text;
        color: green;
        border: 5px outset green!important;
                }
        `;

        // Set cursor position to the end of the suggestion
        textarea.setSelectionRange(cursorPosition + suggestion.length, cursorPosition + suggestion.length);
    }

    function clearSuggestion() {
        if (currentSuggestion) {
            textarea.value = originalValue;
            currentSuggestion = '';
            const styleElement = document.getElementById('suggestion-style');
            if (styleElement) {
                styleElement.textContent = '';
            }
        }
    }

    function acceptSuggestion() {
        if (currentSuggestion) {
            const cursorPosition = textarea.selectionStart;
            const textBefore = textarea.value.substring(0, cursorPosition - currentSuggestion.length);
            const textAfter = textarea.value.substring(cursorPosition);
            textarea.value = textBefore + currentSuggestion + textAfter;
            textarea.setSelectionRange(cursorPosition, cursorPosition);
            currentSuggestion = '';
            const styleElement = document.getElementById('suggestion-style');
            if (styleElement) {
                styleElement.textContent = '';
            }
        }
    }

});
