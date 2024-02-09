/* Copyright (C) 2020 - CODING SRLS */
/* build 10 */


var sections=[];
var other_params=[];
sections=createSections(sections);
other_params=createSections(other_params,".onlyapp");

//definisci la lingua corrente
var url_string = window.location.href;
var url = new URL(url_string);
var current_language = url.searchParams.get("language");
if(current_language==null)
    current_language="it-IT";

console.log("current language:"+current_language);



/* definition of an Object */
function section(name){
    this.name = name;
    this.value=[];
return this;
};

function sectionValue(attrib,value){
    this.attrib = attrib;
    this.value=value;
return this;
};

function newSection(sections,name,attrib,value){
    searchsection=false;
    searchattrib=false;
    sections.forEach(function(item){
        
        if(item.name==name){
            if( Array.isArray(item.value ))
                item.value.forEach(function(itemvalue){
                    if(itemvalue.attrib==attrib){
                        itemvalue.value=value;
                        searchsection=true;
                    }
                })
            else{
                if(item.value.attrib==attrib){
                    item.value.value=value;
                    searchsection=true;
                }
            }
            
        }   
    });

    if(!searchsection){
        //crea una nuova sezione
        var s=new section(name);
        s.value=new sectionValue(attrib,value);
       
        sections.push(s);
    }
    return sections;
}

function removeSection(sections,name,attrib){
    var id_section=0;
    sections.forEach(function(item){
        if(item.name==name){
            if( Array.isArray(item.value ))
                item.value.forEach(function(itemvalue,idx){
                    if(itemvalue.attrib==attrib){
                        item.value.splice(idx,1);
                    }
                })
            else{
                if(item.value.attrib==attrib){
                    delete item.value.attrib;
                    delete item.value.value;
                    
                }
            }
            if(isEmpty(item.value)){
                sections.splice(id_section,1);
            }


            
        }
        id_section++;
    });
}







///Aggiorna tutti i valori HTML con i parametri passati su values
function updateValues(values){

    if(typeof values === 'undefined' || values === null)
        return;

    for(i=0;i<values.length;i++){

        element=values[i];
        
        
        attrib="text";
        value_attrib="";
        //color_attrib="";

        if(Array.isArray(element.value)){
            
            for(j=0;j<element.value.length;j++){
                
                if(element.value[j].attrib=="text"){
                    value_attrib=element.value[j].value;
                    attrib="text";
                }

                if(element.value[j].attrib=="color"){
                    value_attrib=element.value[j].value;
                    attrib="color";
                }

                obj=$('#'+element.name+'[layoutattrib="'+attrib+'"]');
               
                if(value_attrib)
                {

                    if(obj){
                        if (obj.is("input")){
                            obj.val(value_attrib);
                        }
                        else{
                            obj.html(value_attrib);
                        }
                    }
                    if(attrib=="text"){
                        if(typeof(window[element.name])=="number")
                            window[element.name]=parseInt(value_attrib);
                        else
                            window[element.name]=value_attrib;
                    }
                    value_attrib="";
                }

            }
        }else{

            if(element.value.attrib=="text"){
                value_attrib=element.value.value;
                attrib="text";
            }
            if(element.value.attrib=="color"){
                value_attrib=element.value.value;
                attrib="color";

            }

            obj=$('#'+element.name+'[layoutattrib="'+attrib+'"]');
     

            if(value_attrib)
            {

                if(obj){
                    if (obj.is("input")){
                        obj.val(value_attrib);
                    }
                    else{
                        obj.html(value_attrib);
                    }
                }
            
                if(typeof(window[element.name])=="number")
                    window[element.name]=parseInt(value_attrib);
                else
                    window[element.name]=value_attrib;
            }

        }
        
        
        
    }
}

///Richiede al LEDbox i valori di tutte le sezioni presenti sul layout corrente
function requestValues(){

    var message={};
    message.cmd="GetSections";
    message.value="";
    sendCommand(JSON.stringify(message));
}




///Crea il JSON di un sezione

function createSectionValue(name,attrib,value){
    var field={};
    field.name=name;
    field.value={};         
    field.value.attrib=attrib;
    field.value.value=value;
    return field;
}

///Crea il JSON di valori di un sezione
function createValueRow(name,value){
    element={};
    element.name=name;


    element_attrib={}
    element_attrib['attrib']="text";
    element_attrib['value']=value;
    element.value=element_attrib;
    
    return element;
}


/********** HELPER *********************/

///Converte la stringa colori (R,G,B) in esadecimale
function convertRGBColorToHex(color){
    if(color=="")
        return;
    colorsplit=color.split(",");
    return fullColorHex(parseInt(colorsplit[0]), parseInt(colorsplit[1]), parseInt(colorsplit[2])); 

}

//Converte un colore da esadecimale in oggetto Color
function convertColor(color) {
    var rgbColor = {};

    rgbColor.rChannel = parseInt(0,16);
    rgbColor.gChannel = parseInt(0,16);
    rgbColor.bChannel = parseInt(0,16);

    if(color==null)
      return rgbColor;

    if(color.substring(0,1) == '#') {
       color = color.substring(1);
     }            
  
    /* Grab each pair (channel) of hex values and parse them to ints using hexadecimal decoding */
    rgbColor.rChannel = parseInt(color.substring(0,2),16);
    rgbColor.gChannel = parseInt(color.substring(2,4),16);
    rgbColor.bChannel = parseInt(color.substring(4),16);
  
    return rgbColor;
}

//Converte da oggetto Color in esadecimale
var fullColorHex = function(r,g,b) {   
    var red = rgbToHex(r);
    var green = rgbToHex(g);
    var blue = rgbToHex(b);
    return red+green+blue;
  };

///Converte da RGB a Esadecimale
  var rgbToHex = function (rgb) { 
    var hex = Number(rgb).toString(16);
    if (hex.length < 2) {
         hex = "0" + hex;
    }
    return hex;
  };


function sleep(milliseconds) {
    var start = new Date().getTime();
    for (var i = 0; i < 1e7; i++) {
      if ((new Date().getTime() - start) > milliseconds){
        break;
      }
    }
  }


///Emette un beep
function play(keys) {
    var key = keys.shift();
    if(typeof key == 'undefined') return; // song ended
    new Beep(22050).play(key[0], key[1], [Beep.utils.amplify(8000)], function() { play(keys); });
  }

function isEmpty(obj) {
  for(var prop in obj) {
    if(obj.hasOwnProperty(prop)) {
      return false;
    }
  }

  return JSON.stringify(obj) === JSON.stringify({});
}

/********** END HELPER *********************/


function createSections(params,classname){

    if(typeof(classname)=="undefined")
        classname=".layoutledbox";
    
    $(classname).each(function(idx){
        var elementType = $(this).prop('tagName');
        switch(elementType){
            case "INPUT":
                value=$(this).val();
                break;
            default:
                value=$(this).html();
                break;
        }
        params=newSection(params,$(this).attr('name'),$(this).attr('layoutattrib'),value);
    });

    return params;
}


// Aggiorna i valori sul layout del LEDbox
function refreshValue(params){
    if(typeof(params)=="undefined")
        params=sections;         
    params=createSections(params);

    var message={};
    message.cmd="SetSections";
    message.value=params;
    sendCommand(JSON.stringify(message))
    play([[200, 0.1]]);
    save(params);
    other_params=createSections(other_params,".onlyapp");
    save(other_params);
}

//Invia il messaggio all'APP per l'apertura delle playlist
function openAdv(){
    var message={};
    message.cmd="local";
    message.value="openAdv";
    sendCommand(JSON.stringify(message));
}

//Invia il messaggio all'APP per la selezione di un file immagine
function selectImageFile(name){
    var message={};
    message.cmd="local";
    message.name=name;
    message.value="selectImageFile";
    sendCommand(JSON.stringify(message));
}

function uploadToLedbox(local_file){
    var message={};
    message.cmd="local";
    message.name=local_file;
    message.value="uploadToLedbox";
    sendCommand(JSON.stringify(message));
}

//Invia il messaggio per far suonare il buzzer sul LEDbox
function Horn(times,sleep){
    if(typeof(times)=="undefined")
        times=1;
    if(typeof(sleep)=="undefined")
        sleep=0.5;
    
    var message={};
    message.cmd="Horn";
    value={};
    value.times=times;
    value.sleep=sleep;
    message.value=value;
    sendCommand(JSON.stringify(message));

}

//Invia il messaggio per l'apertura di un layout
function openLayout(layout){
    if(layout!=""){        
        var message={};
        message.cmd="SetLayout";
        message.value=layout;
        sendCommand(JSON.stringify(message));
    }                   
}



//Salva le variabili
function save(params){
    // Check browser support
    if (typeof(Storage) !== "undefined") {
      // Store
      params.forEach(function(item){
            if(Array.isArray(item.value)){
                item.value.forEach(function(itemvalue){
                    localStorage.setItem(interface_name+"_"+item.name+"_"+itemvalue.attrib, itemvalue.value);
                });
            }else
                localStorage.setItem(interface_name+"_"+item.name+"_"+item.value.attrib, item.value.value);
        });

    } 

}


//Aggiorna le variabili con 
function load(array_params){
    console.log("Load params");
    array_params.forEach(function(params){
        params.forEach(function(item){
            if(Array.isArray(item.value)){
                    item.value.forEach(function(itemvalue){
                        value=localStorage.getItem(interface_name+"_"+item.name+"_"+itemvalue.attrib);
                        if(value!=null) itemvalue.value=value;
                    });
            }
            else{
                value=localStorage.getItem(interface_name+"_"+item.name+"_"+item.value.attrib);
                if(value!=null) item.value.value=value;
            }
        });
    });
    $.event.trigger({
        type: "afterLoad",
        message: ""
    });
}


function connected(){
    console.log("Ready");
    openLayout(default_layout);
    load([sections,other_params]);
    updateValues(sections);
    updateValues(other_params);
    

}

function getSectionValue(params,name,attrib){
    v="";
    params.forEach(function(item){
        if(item.name==name){
            if(Array.isArray(item.value)){
                
                item.value.forEach(function(item_value){
                    if(item_value.attrib==attrib){
                        v=item_value.value;
                        
                    }
                });
                
            }else{
                if(item.value.attrib==attrib)
                    v= item.value.value;
            }
        }
    });
    return v;
}


function getLanguage(tag){
    l=language[current_language];
    result=tag;
    if(l){
        $.each(l, function(key, value) {
            if(key==tag){
                result=value;
            }
        });
    }

    return result;
}


//Chiude le window aperte
function Back(){
    var wins=$(".messageDialog").toArray();

    for(var i=wins.length-1;i>-1;i--){
        if($(wins[i]).is(":visible")){
            $(wins[i]).hide();
            return;
        }
    }
    
    var message={};
    message.cmd="local";
    message.value="back";
    sendCommand(JSON.stringify(message));
    
    for(var i=0;i<10000;i++){
        clearInterval(i);
        clearTimeout(i);
    }

    
}


ws.onmessage = function(event){
    var m=JSON.parse(event.data);
    
    console.log("RECEIVED: "+event.data)

    switch(m.sender){
        case "Connect":
            connected();
            break;
        
    }

    switch(m.cmd){
		case "SetLayout":
        case "SetSections":
            updateValues(m.value);
            break;

    }
            
    
    $.event.trigger({
        type: "messageReceived",
        message: m
    });


}

function applyLanguage(){
    $("language").each(function(item,index){
        $(this).html(getLanguage($(this).html()));
    });
}

$( document ).ready(function() {
    applyLanguage();
});