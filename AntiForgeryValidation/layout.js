AddAntiForgeryToken = function() {
    //data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
    
    return '__RequestVerificationToken= ' + $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
};