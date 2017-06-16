//校验用户名
function checkUser(str){
    var re = /^[a-zA-z]\w{3,15}$/;
    if(re.test(str)){
        return true;
    }else{
        return false;
    }          
}
/*
校验密码非空
*/
function checkPwd(str){   
    if(str==''){
        return false;
    }else{
        return true;
    }
}