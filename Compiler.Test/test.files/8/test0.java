class firstclass{
  public void firstclass(){
    return;
  }
  public int secondclass(){
    int a,b,c;

    a=b;
    a=5;
    b=10;
    c=a*b;
    return c;
  }
}
final class Main{
  public static void main(String [] args){
    firstclass.secondclass();
  }
}
