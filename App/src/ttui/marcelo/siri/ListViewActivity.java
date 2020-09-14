package ttui.marcelo.siri;


import java.util.LinkedList;
import java.util.List;

import android.app.ListActivity;
import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.view.ContextMenu;
import android.view.ContextMenu.ContextMenuInfo;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

public class ListViewActivity extends ListActivity {
	private String[] descriptions;
	private View lastView;
	@Override
	protected void onResume() {
		super.onResume();
	}
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		List<String> listPower=(List<String>)getIntent().getExtras().get("powers");
		descriptions=new String[listPower.size()];
		int i=0;
		for (String string : listPower) {
			descriptions[i]=string;
			i++;
		}
		setListAdapter(new ArrayAdapter<String>(this, R.layout.list_item,
				descriptions));
		ListView lv = getListView();
		lv.setTextFilterEnabled(true);
		lv.setOnItemClickListener(new OnItemClickListener() {
			public void onItemClick(AdapterView<?> parent, View view,
					int position, long id) {
				if (lastView!=view){
					String name=descriptions[position];
					if (!name.equals("type")){
						Intent returnIntent = new Intent();
						returnIntent.putExtra("result",name);
						setResult(RESULT_OK,returnIntent);
						Toast.makeText(getApplicationContext(),name,Toast.LENGTH_SHORT).show();
						finish();
					}
		
					if (lastView!=null){
						lastView.setBackgroundColor(Color.WHITE);
					}
					view.setBackgroundColor(Color.GRAY);
					lastView=view;
				}
			}
		});
		registerForContextMenu(lv);
	}

	 @Override
	   public void onCreateContextMenu(ContextMenu menu, View v, ContextMenuInfo menuInfo) {
	      super.onCreateContextMenu(menu, v, menuInfo);
	      menu.setHeaderTitle("Colors");
	      menu.add(0, v.getId(), 0, "red");
	      menu.add(0, v.getId(), 0, "blue");
	      menu.add(0, v.getId(), 0, "yellow");
	      menu.add(0, v.getId(), 0, "green");
	      menu.add(0, v.getId(), 0, "white");
	   }
	 
	   @Override
	   public boolean onContextItemSelected(MenuItem item) {
		  String name=item.getTitle().toString();
	      if (name !=null) {
	    	  Intent returnIntent = new Intent();
				returnIntent.putExtra("result","type>"+name);
				setResult(RESULT_OK,returnIntent);
				Toast.makeText(getApplicationContext(),name,Toast.LENGTH_SHORT).show();
				finish();
	      } else return false;
	      return true;
	   }
}