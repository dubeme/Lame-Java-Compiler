class classone{
  public void firstclass(){
    return;
  }
  public int secondclass(int a){
    int b,c;

    b=a;
    b=2*b;
    return b;
  }
  public int thirdclass(){
    int a,b;
    b=5;
    a=classone.secondclass(b);
    writeln(a);
    return 5;
  }
}
final class Main{
  public static void main(String [] args){
    classone.thirdclass();
  }
}
