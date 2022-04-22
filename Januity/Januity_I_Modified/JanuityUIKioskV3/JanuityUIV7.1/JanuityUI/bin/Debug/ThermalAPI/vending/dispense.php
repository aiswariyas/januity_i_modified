<?php 

error_reporting(E_ALL);

$invoice_id = $_REQUEST["invoice_id"];
$payment_id = $_REQUEST["payment_id"];
$mobile_number = $_REQUEST["mobile_number"];
$email = $_REQUEST["email"];
$amount = $_REQUEST["amount"];
$time_stamp = date("Y-m-d H:i:s");
$payment_url = $_REQUEST["payment_url"];
$json_data = $_REQUEST["json_data"];
$created_at = $_REQUEST["created_at"];
$payment_status = $_REQUEST["payment_status"];
$machine_id = $_REQUEST["machine_id"];
$rows = $_REQUEST["rows"];
$quantity = $_REQUEST["quantity"];



//$date = date("Y-m-d"); 
//$time_stamp = date("H:i:s"); 

require('vendornew/autoload.php');

//require 'Net/SSH2.php';

//$ssh = new Net_SSH2('localhost');


$newoutputarray=array();

	use WebSocket\Client;
try{
$client = new Client("ws://localhost:9071/ws");
//$client->send("D");

$ack= $client->receive(); // Will output 'Hello WebSocket.org!'

}catch(Exception $e){
	$ack="Socket Exception";
	
	
}

	//$conn = mysqli_connect('18.219.221.179', 'root', 'Mdimsru', 'rahasiya');	
		//if ($conn->connect_error)
		//{
         			//die("Connection failed: " . $conn->connect_error);
		//}
//$sql = "UPDATE purchase_details SET date_of_transaction='$date',product_id = '$product_id', purchase_cost = '$purchase_cost',time_stamp = '$time_stamp' ,product_name = '$product_name',user_name='$user_name' ,user_number='$user_number' ,user_email='$user_email' ,user_android_id='$user_android_id',user_mobile_model='$user_mobile_model'   WHERE purchase_id='$p_id'";
//$result1 = $GLOBALS['conn']->query($sql);

///*	$sql2 = "INSERT INTO users (user_name,user_number,user_email,user_android_id,user_mobile_model)VALUES ('$user_name','$user_number','$user_email','$user_android_id','$user_mobile_model')";
	//$result2 = $GLOBALS['conn']->query($sql2);*/

	//$sqlnew = "SELECT token FROM purchase_details WHERE purchase_id='$p_id'";
	//$result1 = $GLOBALS['conn']->query($sqlnew);

	//if ($result1->num_rows > 0)
	//{
		//while($row = $result1->fetch_assoc())
		//{
			//$key =  $row["token"];
		//}	
	//}
	//$key_passed = mc_decrypt($token_key, ENCRYPTION_KEY);
	//echo $key_passed;
	//if ($key_passed == $key )
	{
			
if($rows == 'R1')
		{
$client->send("R1".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R1 success";
               }
	       elseif($rows == 'R2')
		{
$client->send("R2".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R2 success";
               }
	       elseif($rows == 'R3')
		{
$client->send("R3".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R3 success";
               }
	       elseif($rows == 'R4')
		{
$client->send("R4".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R4 success";
               }
	       elseif($rows == 'R5')
		{
$client->send("R5".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R5 success";
               }
	       elseif($rows == 'R6')
		{
$client->send("R6".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R6 success";
               }
	       elseif($rows == 'R7')
		{
$client->send("R7".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R7 success";
               }
	       elseif($rows == 'R8')
		{
$client->send("R8".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R8 success";
               }
	       elseif($rows == 'R9')
		{
$client->send("R9".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R9 success";
               }elseif($rows == 'R10')
		{
$client->send("R10".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R10 success";
               }
	       elseif($rows == 'R11')
		{
$client->send("R11".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R11 success";
               }
	       elseif($rows == 'R12')
		{
$client->send("R12".$quantity.$payment_id);
//sleep(16);
$checkStock= $client->receive(); 
$output=$checkStock;
echo "R12 success";
               }
	
else{
$client->send("D");
$output="Socket Exception";	
}


		//$output = $ssh->exec("sudo java -classpath .:classes:/opt/pi4j/lib/'*' Dispense");
		if( !isset( $newoutputarray['output']))
		{
			$newoutputarray['output'] = $output;
		}	
		else
		{
			echo "duplicatekey value";
		}
	}
	
	
	
	
	header('Content-Type: application/json');
	echo json_encode($newoutputarray);
	$GLOBALS['conn']->close();



//function mc_decrypt($decrypt, $key){
    //$decrypt = explode('|', $decrypt.'|');
    //$decoded = base64_decode($decrypt[0]);
    //$iv = base64_decode($decrypt[1]);
    //if(strlen($iv)!==mcrypt_get_iv_size(MCRYPT_RIJNDAEL_256, MCRYPT_MODE_CBC)){ return false; }
    //$key = pack('H*', $key);
    //$decrypted = trim(mcrypt_decrypt(MCRYPT_RIJNDAEL_256, $key, $decoded, MCRYPT_MODE_CBC, $iv));
    //$mac = substr($decrypted, -64);
    //$decrypted = substr($decrypted, 0, -64);
    //$calcmac = hash_hmac('sha256', $decrypted, substr(bin2hex($key), -32));
    //if($calcmac!==$mac){ return false; }
    //$decrypted = unserialize($decrypted);
    //return $decrypted;
//}
?>
