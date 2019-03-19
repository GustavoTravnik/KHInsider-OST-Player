var listGeral = [];
var listAtual = [];
var ostName;
var coreTimer = null;

if ('serviceWorker' in navigator) {
    navigator.serviceWorker
        .register('js/sw.js')
        .then(function (registration) {
            console.log(
                'Service Worker registration successful with scope: ',
                registration.scope
            );
        })
        .catch(function (err) {
            console.log('Service Worker registration failed: ', err);
        });
}

fillGeralOSTList();

function fillGeralOSTList(){    
    $("body").mLoading({ text:"Getting Album List..." });
    if (listGeral.length == 0){
        $.ajax({
            url: "/api/OSTList",
            dataType: 'json',
            complete: function(data, result) {
                listGeral = data.responseJSON;
                $("body").mLoading("hide");
            }
        });
    } else{
        $("body").mLoading("hide");
    }    
}

function findOst(){
    var searchText = $("#geralFinder").val();
    if (searchText.length < 3) {
        alert("The search requires at least 3 characters.");
        return;
    }
    var searchResult = [];
    for (var i = 0; i < listGeral.length; i++){
        var currentLine = listGeral[i];
        if (currentLine.toLowerCase().indexOf(searchText.toLowerCase()) != -1){
            searchResult.push(currentLine);
        }
    }
    $('#geralList').empty();
    searchResult.forEach(function(line){
        $('#geralList').append($('<li> </li>').addClass('list-group-item').text(line).bind('click', swithToAlbumPlayer));
    });   
    if ($('#geralList').children().length == 0) {
        $('#geralList').append($('<li> </li>').addClass('list-group-item').text("No search results..."));
    }
}

function swithToAlbumPlayer(ost){
    ostName = ost.target.textContent;
    var isAlreadyFilled = false;
    isAlreadyFilled = getObjectArrayIndex(ostName) != -1;
    $("body").mLoading({ text:"Getting tracklist...", });
    if (!isAlreadyFilled){
        $.ajax({
            url: "/api/OSTList/" + ostName,
            dataType: 'json',
            complete: function(data) {
                listAtual.push(data.responseJSON);
                createOSTScreen();
            }
        });
    }else{
        createOSTScreen();
    }
}

function createOSTScreen(){
    $("body").mLoading("hide");
    var index = 0;
    index = getObjectArrayIndex(ostName);
    $('#geralList').empty();
    for (var i = 0; i < Object.keys(listAtual[index][ostName]).length; i++){
        $('#geralList').append($('<li> </li>').addClass('list-group-item').text(Object.keys(listAtual[index][ostName])[i]).bind('click', playMusic));
    }
}

function playMusic(track) {
    var trackTarget = track.target == undefined ? track : track.target;
    //get track name by <li> text
    var trackName = trackTarget.textContent;
    //get index of <li> in main parent
    var index = $(trackTarget).prevAll().length;
    //get list of all <li> of <ul>
    var childs = $(trackTarget).parent()[0].children;
    //Paint selected <li> of orange and other of white
    for (var i = 0; i < childs.length; i++) {
        var currentChild = childs[i];
        if (i != index) {
            currentChild.style.backgroundColor = "white";
        } else {
            currentChild.style.backgroundColor = "orange";
        }
    }
    var objectIndex = 0;
    //get index of main OST object list
    objectIndex = getObjectArrayIndex(ostName);
    //get url of track
    var url = listAtual[objectIndex][ostName][trackName];
    //set src and play music
    document.getElementById('audio').src = url;
    document.getElementById('audio').play();
    if (!coreTimer)
        coreTimer = setInterval(autoNextMusic, 1000);
}

function autoNextMusic() {
    if (document.getElementById('audio').ended) {
        var elementIndex = 0;
        var tracklist = document.getElementsByClassName('list-group-item');
        jQuery.grep(tracklist, function (item, index) {
            if (item.style.backgroundColor == 'orange') elementIndex = index+1;
        });
        if (elementIndex == tracklist.length) {
            elementIndex = 0;
        }
        playMusic(tracklist[elementIndex]);
    }
}

function getObjectArrayIndex(name){
    var index = -1;
    for (var i = 0; i < Object.keys(listAtual).length; i++){
        if (listAtual[i][name]){
            index=i;
            break;
        }
    }
    return index;
}