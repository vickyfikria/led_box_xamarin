function LedBoxController() {}

LedBoxController.prototype.processMessage = function (data) {
    var obj = {};
    obj.data = data;
    this.onmessage(obj);
}

var ws = new LedBoxController();

function sendCommand(data) {

    try {
        invokeCSharpAction(data);
    }
    catch (err){
        console.log(err);
    }
}