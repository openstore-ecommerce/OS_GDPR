
$(document).ready(function () {

    // get list of records via ajax:  NBrightRazorTemplate_nbxget({command}, {div of data passed to server}, {return html to this div} )
    nbxget('os_gdpr_getdata', '#selectparams', '#editdata');

    $('.actionbuttonwrapper #cmdsave').unbind('click');
    $('.actionbuttonwrapper #cmdsave').click(function () {
        $('.processing').show();
        nbxget('os_gdpr_savedata', '#editdata');
    });

    $('.actionbuttonwrapper #cmddelete').unbind('click');
    $('.actionbuttonwrapper #cmddelete').click(function () {
        if (confirm($('#deletemsg').val())) {
            $('.processing').show();
            nbxget('os_gdpr_deleterecord', '#editdata');
        }
    });

    $('.selecteditlanguage').unbind('click');
    $('.selecteditlanguage').click(function () {
        $('.processing').show();
        $('#nextlang').val($(this).attr('lang')); // alter lang after, so we get correct data record
        nbxget('os_gdpr_selectlang', '#editdata'); // do ajax call to save current edit form
    });


});

$(document).on("nbxgetcompleted", nbxgetCompleted); // assign a completed event for the ajax calls


function nbxgetCompleted(e) {

    $('.processing').hide();

    if (e.cmd == 'os_gdpr_deleterecord') {
        $('#selecteditemid').val(''); // clear selecteditemid, it now doesn;t exists.
        nbxget('os_gdpr_getdata', '#selectparams', '#editdata');// relist after delete
    }

    if (e.cmd == 'os_gdpr_savedata') {
        $('#selecteditemid').val(''); // clear sleecteditemid.        
        nbxget('os_gdpr_getdata', '#selectparams', '#editdata');// relist after save
    }

    if (e.cmd == 'os_gdpr_selectlang') {
        $('#editlang').val($('#nextlang').val()); // alter lang after, so we get correct data record
        nbxget('os_gdpr_getdata', '#selectparams', '#editdata'); // do ajax call to get edit form
    }

    if (e.cmd == 'os_gdpr_getdata') {
        $('input[datatype=date]').datepicker(); // datepicker for any date fields. Only needed if the data contains date fields. 
    }

}

