export default { 
    onBeforeRequest: requestBuilder => {
        requestBuilder.headers.set('www-authenticate', 'ntlm,negotiate');
    }
}