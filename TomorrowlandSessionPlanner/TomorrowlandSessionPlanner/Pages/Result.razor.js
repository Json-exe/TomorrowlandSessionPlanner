export function saveAsFile(filename, data) {
    let link = document.createElement('a');
    link.download = filename;
    link.href = 'data:application/octet-stream;base64,' + data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}