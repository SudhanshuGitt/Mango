var dataTable;

document.addEventListener("DOMContentLoaded", function () {
    var url = window.location.search;
    if (url.includes("APPROVED")) {
        loadDatatable("APPROVED")
    }
    else if (url.includes("READYFORPICUP")) {
        loadDatatable("READYFORPICUP");
    }
    else if (url.includes("CANCELLED")) {
        loadDatatable("CANCELLED");
    }
    else {
        loadDatatable("ALL");
    }

})

function loadDatatable(status) {
    dataTable = $('#orderTable').DataTable({
        //"processing": true, // for show progress bar
        //"serverSide": true, // for process server side
        //"filter": true, // this is for disable filter (search box)
        //"orderMulti": false,
        "order": [[0, 'desc']],
        "ajax": {
            url: '/order/getallorders?status=' + status,
            "type": "GET",
            "datatype": "json"
        },
        // it will make first colum and not visible and serheable
        //"columnDefs": [{
        //    "targets": [0],
        //    "visible": false,
        //    "searchable": false
        //}],
        "columns": [
            { "data": "orderHeaderId", "name": "orderHeaderId", "autoWidth": true },
            { "data": "email", "name": "Eamil", "autoWidth": true },
            { "data": "name", "name": "Name", "autoWidth": true },
            { "data": "phone", "name": "Phone", "autoWidth": true },
            { "data": "statusName", "name": "Status", "autoWidth": true },
            { "data": "orderTotal", "name": "Total", "autoWidth": true },
            {
                "data": "orderHeaderId",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                    </div>`
                },
                "autoWidth": true
            }
        ]

    })
}
