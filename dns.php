<?php 
$server=$_GET["q"]; 
if (isset($server)) { 
$hosts = gethostbynamel($server); 
foreach ($hosts as $obj) {  
   print "$obj;";  
}  
} else { 
include "index_en.htm"; 
 } 
